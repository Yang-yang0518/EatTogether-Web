<script setup>
import { onMounted, ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { validateState, clearState, getRedirectPath, getAction } from '@/utils/googleOAuth.js' // ✅ 補上 getAction
import { useAuthStore } from '@/stores/auth.js'
import { useToast } from '@/composables/useToast.js'
import apiFetch from '@/utils/apiFetch.js'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import Button from '@/components/common/Button.vue'

const router = useRouter()
const authStore = useAuthStore()
const { show } = useToast()

// ── 狀態 ──────────────────────────────────────────────────
const action = ref('login')

// loading：OAuth 處理中
// error：一般錯誤（state 失敗、後端錯誤）
// blocked：有一般帳號，被阻擋
const pageState = ref('loading')
const errorMessage = ref('')

const loadingMessage = computed(() =>
    action.value === 'link' ? '正在連結 Google 帳號...' : '正在完成 Google 登入...'
)

function goToLogin() {
    router.push('/').then(() => {
        window.dispatchEvent(new CustomEvent('auth:open-modal'))
    })
}

onMounted(async () => {
    const params = new URLSearchParams(window.location.search)
    const code = params.get('code')
    const state = params.get('state')
    const redirectPath = getRedirectPath()
    action.value = getAction() // 讀取 action，同時驅動 loadingMessage

    // state 或 code 不存在，或 state 比對失敗 → 不發 API
    if (!code || !state || !validateState(state)) {
        clearState()
        pageState.value = 'error'
        errorMessage.value = 'Google 登入失敗，請重試'
        return
    }

    clearState()

    // 依 action 分流：連結模式 vs 登入模式
    if (action.value === 'link') {
        // ── 連結模式：呼叫會員中心專用 API，保持原本登入身份 ──
        try {
            const res = await apiFetch('/members/me/google-link', {
                method: 'POST',
                body: JSON.stringify({ code }),
            })

            if (res.ok) {
                await authStore.fetchMe() // 刷新 googleLinked 狀態，但 JWT 不變
                show('Google 帳號連結成功！', 'success')
                router.push('/member')
                return
            }

            const data = await res.json().catch(() => ({}))
            const messages = {
                google_already_linked_to_other: '此 Google 帳號已被其他帳號綁定',
                google_auth_failed: 'Google 驗證失敗，請重試',
                member_blacklisted: '帳號已停權，請聯繫客服',
            }
            pageState.value = 'error'
            errorMessage.value = messages[data.errorCode] || '連結失敗，請稍後再試'
        } catch {
            pageState.value = 'error'
            errorMessage.value = '連結失敗，請重試'
        }
    } else {
        // ── 登入模式 ──
        try {
            const res = await apiFetch('/auth/google/callback', {
                method: 'POST',
                body: JSON.stringify({ code }),
            })

            if (res.ok) {
                const data = await res.json().catch(() => ({}))

                // 一般帳號衝突（同一 Email 已有帳號，但未綁定 Google），提示使用密碼登入
                // 就地顯示阻擋畫面，不跳轉
                if (data.status === 'account_exists_use_password') {
                    pageState.value = 'blocked'
                    return
                }

                // 正常登入成功
                await authStore.fetchMe()
                router.push(redirectPath)
                return
            }

            const data = await res.json().catch(() => ({}))
            const messages = {
                account_blacklisted: '帳號已停權，請聯繫客服',
                login_failed: '登入失敗，請聯絡客服',
            }
            pageState.value = 'error'
            errorMessage.value = messages[data.errorCode] || 'Google 登入失敗，請重試'
        } catch {
            pageState.value = 'error'
            errorMessage.value = 'Google 登入失敗，請重試'
        }
    }
})
</script>

<template>
    <!-- 處理中 -->
    <LoadingSpinner v-if="pageState === 'loading'" :fullscreen="true" :message="loadingMessage" />

    <!-- 一般錯誤 / 一般帳號衝突 -->
    <div
        v-else
        class="d-flex align-items-center justify-content-center"
        style="min-height: 100vh; background: var(--eat-surface)"
    >
        <div
            class="card-eat p-5 text-center d-flex flex-column align-items-center gap-3"
            style="max-width: 420px; width: 100%"
        >
            <!-- 一般錯誤 -->
            <template v-if="pageState === 'error'">
                <div class="icon-eat icon-eat--error">
                    <i class="bi bi-x-lg"></i>
                </div>
                <p class="eat-body mb-0">{{ errorMessage }}</p>
                <Button variant="secondary" class="btn-eat-sm mt-2" @click="router.push('/')">
                    返回首頁
                </Button>
            </template>

            <!-- 一般帳號衝突 -->
            <template v-else-if="pageState === 'blocked'">
                <div class="icon-eat" style="background: var(--eat-secondary-container)">
                    <i class="bi bi-shield-lock" style="color: var(--eat-secondary)"></i>
                </div>
                <p class="eat-h3 fst-normal mb-0">此 Email 已有帳號</p>
                <p class="eat-body-muted mb-0">
                    您的 Google 帳號 Email 已註冊為一般帳號<br />
                    請使用帳號密碼登入，登入後可至會員中心將 Google 帳號加入連結
                </p>
                <Button variant="primary" class="btn-eat-sm mt-2" @click="goToLogin">
                    前往登入
                </Button>
            </template>
        </div>
    </div>
</template>
