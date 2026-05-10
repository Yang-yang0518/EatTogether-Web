<script setup>
import { ref, computed, onMounted, onUnmounted, onBeforeUnmount } from 'vue'
import { useAuthStore } from '@/stores/auth.js'
import { useToast } from '@/composables/useToast.js'
import apiFetch from '@/utils/apiFetch.js'
import { validateImageFile, previewImage } from '@/utils/imageUpload.js'
import { generateState, buildGoogleOAuthUrl } from '@/utils/googleOAuth.js'
import AvatarInitial from '@/components/member/AvatarInitial.vue'
import FormErrorMessage from '@/components/common/FormErrorMessage.vue'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import ChangeEmailModal from '@/components/member/modals/ChangeEmailModal.vue'
import ChangePasswordModal from '@/components/member/modals/ChangePasswordModal.vue'
import CreateAccountModal from '@/components/member/modals/CreateAccountModal.vue'
import DeleteAccountModal from '@/components/member/modals/DeleteAccountModal.vue'
import BirthDateConfirmModal from '@/components/member/modals/BirthDateConfirmModal.vue'
import Button from '@/components/common/Button.vue'

const authStore = useAuthStore()
const { show: showToast } = useToast()

// ── 基本資料表單（phone/birthDate 不放 authStore）─────────
const profileName = ref('')
const profilePhone = ref('')
const profileBirthDate = ref(null) // null 初始值，VueDatePicker onMounted 不觸發 format
const nameError = ref('')
const phoneError = ref('')
const isSavingProfile = ref(false)

// ── 頭像上傳 ──────────────────────────────────────────────
const previewUrl = ref(null)
const isUploading = ref(false)
const fileInput = ref(null)

// ── Email 變更提示 ────────────────────────────────────────
const emailChangeSent = ref(false)

let isMounted = true
let birthDateTimer = null

const unlinkingProvider = ref(null) // 記錄目前正在取消連結的 provider key

const showBirthDateConfirmModal = ref(false)

const canUnlinkGoogle = computed(() => {
    const { hashedPasswordStatus, googleLinked } = authStore.member
    if (hashedPasswordStatus === 'HAS_PASSWORD') return true
    // 沒有密碼時，計算目前連結的第三方數量
    // 未來擴充時在陣列裡加入新的第三方欄位即可
    const linkedCount = [
        googleLinked,
        // authStore.member.lineLinked,
        // authStore.member.facebookLinked,
    ].filter(Boolean).length
    return linkedCount > 1
})

const thirdPartyProviders = computed(() => [
    {
        key: 'google',
        label: 'Google',
        icon: 'bi bi-google',
        linked: authStore.member.googleLinked,
        linkedEmail: authStore.member.googleEmail,
        canUnlink: canUnlinkGoogle.value,
        canUnlinkHint: '請先建立一般帳號或連結其他第三方帳號，才能取消連結',
        isUnlinking: unlinkingProvider.value === 'google',
        onLink: linkGoogle,
        onUnlink: unlinkGoogle,
    },
    // 未來擴充範例：
    // {
    //     key: 'line',
    //     label: 'LINE',
    //     icon: 'bi bi-line',
    //     linked: authStore.member.lineLinked,
    //     canUnlink: canUnlinkLine.value,
    //     canUnlinkHint: '請先建立一般帳號或連結其他第三方帳號，才能取消連結',
    //     onLink: linkLine,
    //     onUnlink: unlinkLine,
    // },
])

// ── 工具：Date → YYYY-MM-DD（避免 toISOString 時區偏移）──
function toDateString(date) {
    if (!date) return null
    const d = new Date(date)
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

// ── 初始化：取得完整會員資料（含 phone/birthDate）─────────
async function loadProfile() {
    try {
        const res = await apiFetch('/members/me')
        if (!res.ok) return
        const data = await res.json()
        if (!isMounted) return
        profileName.value = data.name
        profilePhone.value = data.phone ?? ''
        // setTimeout (macrotask) 保證在 VueDatePicker onMounted 之後才設值
        if (data.birthDate) {
            birthDateTimer = setTimeout(() => {
                if (!isMounted) return
                profileBirthDate.value = new Date(data.birthDate + 'T00:00:00')
            }, 0)
        }
    } catch (err) {
        if (import.meta.env.DEV) console.error('[loadProfile]', err)
    }
}

// ── 頭像上傳 ──────────────────────────────────────────────
function triggerFileInput() {
    fileInput.value?.click()
}

async function handleFileChange(event) {
    const file = event.target.files?.[0]
    if (!file) return
    event.target.value = '' // 清除 input，讓同一檔案可重複觸發

    const validation = validateImageFile(file)
    if (!validation.valid) {
        showToast(validation.error, 'error')
        return
    }

    const objectUrl = previewImage(file)
    previewUrl.value = objectUrl
    await uploadAvatar(file, objectUrl)
}

async function uploadAvatar(file, objectUrl) {
    isUploading.value = true
    try {
        const formData = new FormData()
        formData.append('file', file)

        const res = await apiFetch('/members/me/avatar', {
            method: 'POST',
            body: formData,
        })

        URL.revokeObjectURL(objectUrl)
        previewUrl.value = null

        if (res.ok) {
            await authStore.fetchMe()
            showToast('頭像已更新', 'success')
            return
        }

        if (res.status < 500) {
            const data = await res.json()
            showToast(data?.message || '上傳失敗，請稍後再試', 'error')
        }
    } catch (err) {
        URL.revokeObjectURL(objectUrl)
        previewUrl.value = null
        if (import.meta.env.DEV) console.error('[uploadAvatar]', err)
    } finally {
        isUploading.value = false
    }
}

// ── 基本資料 ──────────────────────────────────────────────
async function saveProfile() {
    nameError.value = ''
    phoneError.value = ''

    const trimmedName = profileName.value.trim()
    if (!trimmedName) {
        nameError.value = '姓名為必填'
        return
    }
    if (trimmedName.length > 50) {
        nameError.value = '姓名最多 50 字元'
        return
    }
    if (profilePhone.value && !/^\d{10}$/.test(profilePhone.value)) {
        phoneError.value = '手機格式不正確（請輸入 10 位數字）'
        return
    }

    // 原本沒有生日，這次填了 → 跳確認 Modal
    if (!authStore.member.birthDate && profileBirthDate.value) {
        showBirthDateConfirmModal.value = true
        return
    }

    await doSaveProfile()
}

async function doSaveProfile() {
    isSavingProfile.value = true
    try {
        const trimmedName = profileName.value.trim()
        const res = await apiFetch('/members/me/profile', {
            method: 'PUT',
            body: JSON.stringify({
                name: trimmedName,
                phone: profilePhone.value || null,
                birthDate: toDateString(profileBirthDate.value),
            }),
        })

        if (res.ok) {
            await authStore.fetchMe()
            showToast('個人資料已更新', 'success')
            return
        }

        if (res.status < 500) {
            const data = await res.json()
            if (data?.errorCode === 'invalid_birth_date') {
                showToast('生日不可為未來日期', 'error')
            } else if (data?.errorCode === 'birth_date_locked') {
                showToast('生日設定後無法修改', 'error')
            } else {
                showToast(data?.message || '更新失敗，請稍後再試', 'error')
            }
        }
    } catch (err) {
        if (import.meta.env.DEV) console.error('[doSaveProfile]', err)
    } finally {
        isSavingProfile.value = false
    }
}

function onBirthDateConfirmed() {
    showBirthDateConfirmModal.value = false
    doSaveProfile()
}

function onBirthDateCancelled() {
    showBirthDateConfirmModal.value = false
    // profileBirthDate 保留，讓使用者可以修改後再送
}

// ── Google 連結 ───────────────────────────────────────────
async function unlinkGoogle() {
    unlinkingProvider.value = 'google'
    try {
        const res = await apiFetch('/members/me/google-link', { method: 'DELETE' })

        if (res.ok) {
            await authStore.fetchMe()
            showToast('已取消與 Google 帳號的連結', 'success')
            return
        }

        if (res.status < 500) {
            const data = await res.json()
            showToast(data?.message || '取消連結失敗，請稍後再試', 'error')
        }
    } catch (err) {
        if (import.meta.env.DEV) console.error('[unlinkGoogle]', err)
    } finally {
        unlinkingProvider.value = null
    }
}

function linkGoogle() {
    const state = generateState('/member', 'link') // 傳入 action = 'link'
    const url = buildGoogleOAuthUrl(state)
    window.location.href = url
}

// ── Modal success handlers ────────────────────────────────
function onEmailChangeSuccess() {
    emailChangeSent.value = true
}

function onPasswordChangeSuccess() {
    // Toast 由 ChangePasswordModal 處理，此處無需額外動作
}

async function onCreateAccountSuccess() {
    await authStore.fetchMe()
}

// ── Lifecycle ─────────────────────────────────────────────
onMounted(async () => {
    isMounted = true
    loadProfile()
})

onBeforeUnmount(() => {
    isMounted = false
    clearTimeout(birthDateTimer)
})

onUnmounted(() => {
    // 釋放頭像預覽 Object URL，避免記憶體洩漏
    if (previewUrl.value) {
        URL.revokeObjectURL(previewUrl.value)
    }
})
</script>

<template>
    <!-- 主內容 -->
    <div class="d-flex flex-column gap-4">
        <!-- ── 頭像區塊 ── -->
        <div class="card-eat p-4 text-center">
            <div class="mb-3 d-flex justify-content-center">
                <div v-if="isUploading">
                    <LoadingSpinner size="sm" message="上傳中..." />
                </div>
                <img
                    v-else-if="previewUrl"
                    :src="previewUrl"
                    alt="頭像預覽"
                    class="avatar-preview"
                />
                <AvatarInitial
                    v-else
                    :avatarFileName="authStore.member.avatarFileName"
                    :googleAvatarUrl="authStore.member.googleAvatarUrl"
                    :name="authStore.member.name"
                    size="100px"
                    interactive
                    @click="triggerFileInput"
                />
            </div>
            <input
                ref="fileInput"
                type="file"
                accept="image/jpeg,image/png,image/webp"
                class="d-none"
                @change="handleFileChange"
            />
            <button
                type="button"
                class="eat-body-muted"
                style="background: none; border: none; cursor: pointer"
                @click="triggerFileInput"
            >
                更換頭像
            </button>
        </div>

        <!-- ── 基本資料 ── -->
        <div class="card-eat p-4">
            <h2 class="eat-h3">基本資料</h2>
            <div class="feather-divider on-container mt-2 mb-4"></div>
            <div class="form-eat">
                <div class="d-flex flex-column gap-3">
                    <!-- 姓名 -->
                    <div>
                        <label class="form-label">姓名</label>
                        <input
                            v-model="profileName"
                            type="text"
                            class="form-control"
                            placeholder="請輸入姓名"
                            autocomplete="name"
                        />
                        <FormErrorMessage :message="nameError" :show="!!nameError" />
                    </div>
                    <!-- 手機 -->
                    <div>
                        <label class="form-label">手機</label>
                        <input
                            v-model="profilePhone"
                            type="tel"
                            class="form-control"
                            placeholder="0912345678"
                            autocomplete="tel"
                        />
                        <FormErrorMessage :message="phoneError" :show="!!phoneError" />
                    </div>
                    <!-- 生日 -->
                    <div>
                        <!-- 已設定過：唯讀 -->
                        <template v-if="authStore.member.birthDate">
                            <input
                                class="form-control"
                                :value="toDateString(profileBirthDate)"
                                disabled
                            />
                            <small class="eat-body-muted">生日設定後無法修改</small>
                        </template>
                        <!-- 未設定：可編輯 -->
                        <template v-else>
                            <VueDatePicker
                                v-model="profileBirthDate"
                                :max-date="new Date()"
                                :enable-time-picker="false"
                                :formats="{ input: 'yyyy/MM/dd' }"
                                auto-apply
                            />
                            <small class="eat-body-muted"
                                >生日設定後將無法修改，請確認後再儲存</small
                            >
                        </template>
                    </div>
                </div>
                <Button
                    variant="primary"
                    class="btn-eat-sm mt-4"
                    :loading="isSavingProfile"
                    @click="saveProfile"
                >
                    {{ isSavingProfile ? '儲存中...' : '儲存變更' }}
                </Button>
            </div>
        </div>

        <!-- ── Email 設定 ── -->
        <div class="card-eat p-4">
            <h2 class="eat-h3">Email 設定</h2>
            <div class="feather-divider on-container mt-2 mb-4"></div>
            <div class="form-eat">
                <label class="form-label">目前 Email</label>
                <input class="form-control" :value="authStore.member.email" disabled />
            </div>
            <p v-if="emailChangeSent" class="eat-body-muted mt-2 mb-0">
                驗證信已寄出，請至新信箱點擊連結完成變更
            </p>
            <span
                :title="
                    authStore.member.hashedPasswordStatus === 'EXTERNAL_LOGIN_NO_PASSWORD'
                        ? '第三方登入帳號的 Email 無法修改'
                        : undefined
                "
                style="display: inline-block"
            >
                <Button
                    variant="secondary"
                    class="btn-eat-sm mt-3"
                    :disabled="
                        authStore.member.hashedPasswordStatus === 'EXTERNAL_LOGIN_NO_PASSWORD'
                    "
                    data-bs-toggle="modal"
                    data-bs-target="#changeEmailModal"
                >
                    修改 Email
                </Button>
            </span>
        </div>

        <!-- ── 登入方式 ── -->
        <div class="card-eat p-4 d-flex flex-column gap-4">
            <div>
                <h2 class="eat-h3 mb-1">登入方式</h2>
                <div class="feather-divider on-container mt-2 mb-0"></div>
            </div>

            <!-- 一般帳號列 -->
            <div
                class="d-flex flex-column flex-md-row justify-content-between align-items-md-center align-items-start gap-3"
            >
                <div class="d-flex align-items-center gap-3">
                    <div class="login-method-icon">
                        <i class="bi bi-person-fill"></i>
                    </div>
                    <div>
                        <template v-if="authStore.member.hashedPasswordStatus === 'HAS_PASSWORD'">
                            <div class="login-method-label">一般帳號</div>
                            <div style="font-size: 0.9rem">
                                已建立帳號
                                <span class="text-eat-primary fw-bold">{{
                                    authStore.member.account
                                }}</span>
                            </div>
                        </template>
                        <template v-else>
                            <div class="login-method-label">一般帳號</div>
                            <div class="login-method-hint">尚未建立</div>
                        </template>
                    </div>
                </div>
                <div>
                    <template v-if="authStore.member.hashedPasswordStatus === 'HAS_PASSWORD'">
                        <Button
                            variant="secondary"
                            class="btn-eat-sm"
                            data-bs-toggle="modal"
                            data-bs-target="#changePasswordModal"
                        >
                            修改密碼
                        </Button>
                    </template>
                    <template v-else>
                        <Button
                            variant="primary"
                            class="btn-eat-sm"
                            data-bs-toggle="modal"
                            data-bs-target="#createAccountModal"
                        >
                            建立帳號
                        </Button>
                    </template>
                </div>
            </div>

            <!-- 第三方登入列（v-for 支援未來擴充） -->
            <template v-for="provider in thirdPartyProviders" :key="provider.key">
                <div
                    class="d-flex flex-column flex-md-row justify-content-between align-items-md-center align-items-start gap-3"
                >
                    <div class="d-flex align-items-center gap-3">
                        <div class="login-method-icon">
                            <i :class="provider.icon"></i>
                        </div>
                        <div>
                            <div class="login-method-label">{{ provider.label }} 帳號</div>
                            <div
                                class="login-method-hint"
                                :style="provider.linked ? 'color: var(--eat-primary)' : ''"
                            >
                                <template v-if="provider.linked">
                                    已連結
                                    <span
                                        v-if="provider.linkedEmail"
                                        class="eat-body-muted"
                                        style="font-size: 0.8rem"
                                    >
                                        {{ provider.linkedEmail }}
                                    </span>
                                </template>
                                <template v-else>尚未連結</template>
                            </div>
                        </div>
                    </div>
                    <div>
                        <template v-if="provider.linked">
                            <span
                                :title="!provider.canUnlink ? provider.canUnlinkHint : undefined"
                                style="display: inline-block"
                            >
                                <Button
                                    variant="secondary"
                                    class="btn-eat-sm"
                                    :disabled="!provider.canUnlink"
                                    :loading="provider.isUnlinking"
                                    :aria-disabled="!provider.canUnlink ? 'true' : undefined"
                                    @click="provider.onUnlink"
                                >
                                    取消連結
                                </Button>
                            </span>
                        </template>
                        <template v-else>
                            <Button variant="secondary" class="btn-eat-sm" @click="provider.onLink">
                                前往連結
                            </Button>
                        </template>
                    </div>
                </div>
            </template>
        </div>

        <!-- ── 危險操作 ── -->
        <div class="card-eat p-4" style="border-color: var(--eat-error)">
            <h2 class="eat-h3 mb-1" style="color: var(--eat-error)">危險操作</h2>
            <div class="feather-divider divider-danger on-container mt-2 mb-4"></div>
            <p class="eat-body-muted mb-0">停用帳號後可在登入時選擇重新啟用</p>
            <Button
                variant="danger"
                class="btn-eat-sm mt-3"
                data-bs-toggle="modal"
                data-bs-target="#deleteAccountModal"
            >
                刪除帳號
            </Button>
        </div>
    </div>

    <!-- ════════════════ Modals ════════════════ -->
    <ChangeEmailModal :currentEmail="authStore.member.email" @success="onEmailChangeSuccess" />
    <ChangePasswordModal @success="onPasswordChangeSuccess" />
    <CreateAccountModal @success="onCreateAccountSuccess" />
    <DeleteAccountModal :hasPassword="authStore.member.hashedPasswordStatus === 'HAS_PASSWORD'" />
    <BirthDateConfirmModal
        v-if="showBirthDateConfirmModal"
        :birthDate="toDateString(profileBirthDate)"
        :isLoading="isSavingProfile"
        @confirm="onBirthDateConfirmed"
        @cancel="onBirthDateCancelled"
    />
</template>

<style scoped>
.avatar-preview {
    width: 100px;
    height: 100px;
    border-radius: 50%;
    object-fit: cover;
}
.card-eat {
    border-radius: var(--eat-radius);
    padding: 1.5rem;
    box-shadow: 0 2px 8px rgba(var(--eat-primary), 0.05);
}
.eat-h3 {
    font-size: 1.25rem;
    font-style: normal;
}

.feather-divider {
    background: linear-gradient(90deg, var(--eat-secondary), transparent);
}

.feather-divider.divider-danger {
    background: linear-gradient(90deg, var(--eat-error), transparent);
}

.feather-divider::after {
    content: '';
}

.login-method-row {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 1rem;
}

.login-method-icon {
    width: 40px;
    height: 40px;
    border-radius: var(--eat-radius);
    background: var(--eat-surface);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.25rem;
    color: var(--eat-on-surface-variant);
    flex-shrink: 0;
}

.login-method-label {
    font-size: 0.875rem;
    color: var(--eat-on-surface-variant);
    margin-bottom: 0.1rem;
}

.login-method-hint {
    font-size: 0.9rem;
    color: var(--eat-on-surface-variant);
}
</style>
