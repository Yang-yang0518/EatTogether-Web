<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { Modal } from 'bootstrap'
import apiFetch from '@/utils/apiFetch.js'
import Button from '@/components/common/Button.vue'
import FormErrorMessage from '@/components/common/FormErrorMessage.vue'

const route = useRoute()
const router = useRouter()

// 'loading' | 'success' | 'invalid'
const status = ref('loading')

// 成功狀態：倒數計時
const countdown = ref(3)

// 失效狀態：重寄驗證信
const resendEmail = ref('')
const resendEmailError = ref('')
const resendLoading = ref(false)
const resendDone = ref(false)
const resendMessage = ref('')
const resendErrorCode = ref('')

function startCountdown() {
    const timer = setInterval(() => {
        countdown.value -= 1
        if (countdown.value <= 0) {
            clearInterval(timer)
            router.push({ name: 'Home' }).then(() => {
                const modalEl = document.querySelector('#authModal')
                const modal = Modal.getInstance(modalEl) || new Modal(modalEl)
                modal.show()
            })
        }
    }, 1000)
}

function validateResendEmail() {
    resendEmailError.value = ''
    const trimmed = resendEmail.value.trim()
    if (!trimmed) {
        resendEmailError.value = 'Email 為必填'
        return false
    }
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(trimmed)) {
        resendEmailError.value = 'Email 格式不正確'
        return false
    }
    return true
}

async function handleResend() {
    if (!validateResendEmail()) return
    resendLoading.value = true
    resendMessage.value = ''
    resendErrorCode.value = ''
    try {
        const res = await apiFetch('/auth/resend-verify-email', {
            method: 'POST',
            body: JSON.stringify({ email: resendEmail.value.trim() }),
        })

        if (res.status >= 500) return

        const data = await res.json()
        resendMessage.value = data.message || '驗證信已寄出，請至信箱點擊驗證連結'
        resendErrorCode.value = data.errorCode || ''
        resendDone.value = true
    } catch {
        // 網路錯誤由 apiFetch 統一 Toast 處理
    } finally {
        resendLoading.value = false
    }
}

onMounted(async () => {
    const token = route.query.token
    if (!token) {
        status.value = 'invalid'
        return
    }
    try {
        const res = await apiFetch(`/auth/verify-email?token=${encodeURIComponent(token)}`)
        if (res.ok) {
            status.value = 'success'
            startCountdown()
        } else {
            status.value = 'invalid'
        }
    } catch {
        status.value = 'invalid'
    }
})
</script>

<template>
    <main class="verify-email-main d-flex align-items-center">
        <div class="container">
            <div class="row justify-content-center">
                <div class="col-12 col-sm-10 col-md-7 col-lg-5">
                    <div class="verify-card">
                        <!-- 驗證中 -->
                        <div v-if="status === 'loading'" class="text-center py-4">
                            <div class="spinner-border verify-spinner mb-3" role="status">
                                <span class="visually-hidden">驗證中...</span>
                            </div>
                            <p class="eat-body mb-0">驗證中，請稍候...</p>
                        </div>

                        <!-- 驗證成功 -->
                        <div v-else-if="status === 'success'" class="text-center py-4">
                            <div class="icon-eat icon-eat--success mb-3">
                                <i class="bi bi-check-lg"></i>
                            </div>
                            <h1 class="eat-h3 fw-bolder fst-normal mb-2">Email 驗證成功！</h1>
                            <p class="eat-body-muted mb-3">您的帳號已完成驗證，歡迎加入義起吃</p>
                            <p class="eat-body-muted mb-0">
                                {{ countdown }} 秒後自動導向首頁並開啟登入...
                            </p>
                        </div>

                        <!-- 驗證失效 -->
                        <div v-else class="py-2">
                            <div class="text-center mb-4">
                                <div class="icon-eat icon-eat--error mb-3">
                                    <i class="bi bi-x-lg"></i>
                                </div>
                                <h1 class="eat-h3 fw-bolder fst-normal mb-2">
                                    驗證連結無效或已過期
                                </h1>
                                <p class="eat-body-muted mb-3">請輸入您的 Email，重新寄送驗證信</p>
                            </div>

                            <!-- 重寄表單 -->
                            <div v-if="!resendDone" class="form-eat">
                                <div class="mb-3">
                                    <label for="resend-email" class="eat-label d-block mb-1"
                                        >Email</label
                                    >
                                    <input
                                        id="resend-email"
                                        v-model="resendEmail"
                                        type="email"
                                        class="form-control"
                                        placeholder="請輸入您的 Email"
                                        :disabled="resendLoading"
                                        @blur="validateResendEmail"
                                        autocomplete="email"
                                        :aria-describedby="
                                            resendEmailError ? 'resend-email-error' : undefined
                                        "
                                        :aria-invalid="resendEmailError ? 'true' : undefined"
                                    />
                                    <FormErrorMessage
                                        :show="!!resendEmailError"
                                        :message="resendEmailError"
                                    />
                                </div>
                                <Button
                                    variant="primary"
                                    class="w-100 fs-6 py-2 mt-3"
                                    :loading="resendLoading"
                                    @click="handleResend"
                                >
                                    {{ resendLoading ? '寄送中...' : '重寄驗證信' }}
                                </Button>
                            </div>

                            <!-- 重寄結果 -->
                            <div v-else class="text-center pt-2">
                                <p
                                    class="eat-body-muted mb-0"
                                    :style="
                                        resendErrorCode === 'already_confirmed'
                                            ? 'color: var(--eat-primary)'
                                            : ''
                                    "
                                >
                                    {{ resendMessage }}
                                </p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </main>
</template>

<style scoped>
.verify-email-main {
    min-height: calc(100vh - 80px - 80px);
    padding: 3rem 0;
}

.verify-card {
    background: var(--eat-surface-container);
    border: 1px solid var(--eat-outline-variant);
    border-radius: var(--eat-radius-lg);
    padding: 2.5rem 2rem;
}

.verify-spinner {
    color: var(--eat-primary);
    width: 3rem;
    height: 3rem;
}
</style>
