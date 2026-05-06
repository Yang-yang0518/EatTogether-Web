<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth.js'
import { useToast } from '@/composables/useToast.js'
import apiFetch from '@/utils/apiFetch.js'
import FormErrorMessage from '@/components/common/FormErrorMessage.vue'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import Button from '@/components/common/Button.vue'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const { show: showToast } = useToast()

// ── 狀態 ──────────────────────────────────────────────────
// 'loading' | 'ready' | 'invalid' | 'done'
const pageState = ref('loading')

const password = ref('')
const showPassword = ref(false)
const passwordError = ref('')
const isSubmitting = ref(false)

const token = route.query.token

// 是否需要顯示密碼欄位
// 已登入且確認為純 Google 帳號才隱藏；其餘情況預設顯示（保守策略）
const needsPassword = computed(() => {
    if (!authStore.isLoggedIn) return true
    return authStore.member.hashedPasswordStatus === 'HAS_PASSWORD'
})

// ── 送出確認刪除 ──────────────────────────────────────────
async function handleSubmit() {
    passwordError.value = ''

    if (needsPassword.value && !password.value) {
        passwordError.value = '請輸入密碼以確認刪除'
        return
    }

    isSubmitting.value = true
    try {
        const res = await apiFetch('/members/confirm-delete', {
            method: 'DELETE',
            body: JSON.stringify({
                token,
                password: needsPassword.value ? password.value : null,
            }),
        })

        if (res.ok) {
            pageState.value = 'done'
            authStore.clearAuth()
            showToast('帳號已成功刪除，感謝您使用我們的服務', 'success')
            router.push('/')
            return
        }

        if (res.status < 500) {
            const data = await res.json()
            if (data?.errorCode === 'wrong_password') {
                passwordError.value = '密碼不正確'
            } else if (data?.errorCode === 'password_required') {
                passwordError.value = '請輸入密碼'
            } else if (
                data?.errorCode === 'invalid_or_expired_token' ||
                data?.errorCode === 'invalid_token_type'
            ) {
                pageState.value = 'invalid'
            } else {
                showToast(data?.message || '刪除失敗，請稍後再試', 'error')
            }
        }
    } catch (err) {
        if (import.meta.env.DEV) console.error('[ConfirmDeleteAccount.handleSubmit]', err)
    } finally {
        isSubmitting.value = false
    }
}

// ── 初始化 ────────────────────────────────────────────────
onMounted(async () => {
    // token 不存在直接顯示失效畫面
    if (!token) {
        pageState.value = 'invalid'
        return
    }

    // 嘗試確認登入狀態，用於判斷是否需要密碼欄位
    // 失敗也沒關係，needsPassword 會走保守策略（預設顯示密碼欄）
    await authStore.checkAuth()
    pageState.value = 'ready'
})
</script>

<template>
    <div class="d-flex justify-content-center align-items-center" style="min-height: 60vh">
        <!-- Loading -->
        <div v-if="pageState === 'loading'">
            <LoadingSpinner message="驗證連結中..." />
        </div>

        <!-- 連結失效 -->
        <div
            v-else-if="pageState === 'invalid'"
            class="card-eat p-5 text-center"
            style="max-width: 420px; width: 100%"
        >
            <i class="bi bi-x-circle fs-1 mb-3" style="color: var(--eat-error)"></i>
            <h2 class="eat-h3 fw-bolder fst-normal fs-5 mb-2">連結已失效</h2>
            <p class="eat-body-muted mb-4">此刪除連結已過期或已使用，請重新申請</p>
            <Button variant="secondary" class="btn-eat-sm" @click="router.push('/')">
                返回首頁
            </Button>
        </div>

        <!-- 確認刪除表單 -->
        <div
            v-else-if="pageState === 'ready'"
            class="card-eat p-5"
            style="max-width: 420px; width: 100%"
        >
            <h2 class="eat-h3 fw-bolder fst-normal fs-5 mb-2 text-center">確認刪除帳號</h2>
            <p class="eat-body-muted mb-1 text-center">
                確認後帳號將會停用
                <br />
                若日後想恢復，可在登入時選擇重新啟用帳號
            </p>

            <div v-if="needsPassword" class="form-eat mt-3">
                <label for="cda-password" class="form-label">請輸入密碼以確認刪除</label>
                <div class="position-relative">
                    <input
                        id="cda-password"
                        v-model="password"
                        :type="showPassword ? 'text' : 'password'"
                        class="form-control"
                        :class="{ 'is-invalid': passwordError }"
                        placeholder="請輸入目前密碼"
                        autocomplete="current-password"
                        :disabled="isSubmitting"
                        @keyup.enter="handleSubmit"
                        :aria-describedby="passwordError ? 'cda-password-error' : undefined"
                        :aria-invalid="passwordError ? 'true' : undefined"
                    />
                    <button
                        type="button"
                        class="btn-eat-password-toggle"
                        @mousedown.prevent
                        @click="showPassword = !showPassword"
                        :aria-label="showPassword ? '隱藏密碼' : '顯示密碼'"
                    >
                        <i :class="showPassword ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
                    </button>
                </div>
                <FormErrorMessage
                    id="cda-password-error"
                    :message="passwordError"
                    :show="!!passwordError"
                />
            </div>

            <Button
                variant="danger"
                class="btn-eat-md mt-2"
                :loading="isSubmitting"
                @click="handleSubmit"
            >
                {{ isSubmitting ? '刪除中...' : '確認刪除帳號' }}
            </Button>
        </div>
    </div>
</template>
