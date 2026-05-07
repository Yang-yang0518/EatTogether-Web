<script setup>
import { onMounted } from 'vue'
import Button from '@/components/common/Button.vue'
import { useForgotPassword } from '@/composables/useForgotPassword.js'
import FormErrorMessage from '@/components/common/FormErrorMessage.vue'

const { status, email, emailError, isLoading, validateEmail, handleSubmit, resetState } =
    useForgotPassword()

onMounted(() => {
    const modalEl = document.querySelector('#resendResetPasswordModal')
    modalEl.addEventListener('hidden.bs.modal', resetState)
})
</script>

<template>
    <div
        id="resendResetPasswordModal"
        class="modal fade"
        data-bs-backdrop="static"
        data-bs-keyboard="false"
        tabindex="-1"
        aria-labelledby="resendResetPasswordModalLabel"
        aria-hidden="true"
    >
        <div class="modal-dialog modal-dialog-centered modal-fullscreen-sm-down modal-eat-dialog">
            <div class="modal-content modal-eat-content">
                <button
                    type="button"
                    class="btn-close btn-close-white btn-sm me-2 mt-2 ms-auto"
                    data-bs-dismiss="modal"
                    aria-label="關閉"
                ></button>

                <div class="modal-header border-0 px-4 py-0">
                    <div class="w-100 d-flex justify-content-center">
                        <h2
                            id="resendResetPasswordModalLabel"
                            class="eat-h3 fw-bolder fst-normal fs-5 mb-0"
                        >
                            {{ status === 'form' ? '重新申請重設密碼' : '密碼重設信已寄出' }}
                        </h2>
                    </div>
                </div>

                <div class="modal-body px-4 pt-3 pb-4">
                    <!-- 狀態一：輸入 Email 表單 -->
                    <div v-if="status === 'form'" class="d-flex flex-column gap-2">
                        <p class="eat-body-muted mb-1 text-center" style="font-size: 0.9rem">
                            請輸入您的 Email，我們將寄送密碼重設信給您
                        </p>

                        <div>
                            <label for="rr-modal-email" class="form-label d-block mb-1">
                                Email
                            </label>
                            <input
                                id="rr-modal-email"
                                v-model="email"
                                type="email"
                                class="form-control w-100"
                                :class="{ 'is-invalid': emailError }"
                                placeholder="請輸入您的 Email"
                                autocomplete="email"
                                :disabled="isLoading"
                                @blur="validateEmail"
                                @keyup.enter="handleSubmit"
                                :aria-describedby="emailError ? 'rr-modal-email-error' : undefined"
                                :aria-invalid="emailError ? 'true' : undefined"
                            />
                            <FormErrorMessage
                                id="rr-modal-email-error"
                                :show="!!emailError"
                                :message="emailError"
                            />
                        </div>

                        <Button
                            variant="primary"
                            class="btn-eat-md mt-2 mb-4"
                            :loading="isLoading"
                            @click="handleSubmit"
                        >
                            {{ isLoading ? '寄送中...' : '寄送重設信' }}
                        </Button>
                    </div>

                    <!-- 狀態二：寄送成功 -->
                    <div v-else class="text-center py-2">
                        <div class="icon-eat icon-eat--success mb-3">
                            <i class="bi bi-envelope"></i>
                        </div>
                        <p class="eat-body-muted mb-0" role="alert" style="font-size: 0.9rem">
                            請檢查您的收件匣<br />
                            若未收到，請確認 Email 是否正確<br />
                            或檢查垃圾郵件資料夾
                        </p>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.eat-link-btn {
    background: none;
    border: none;
    color: var(--eat-primary);
    font-family: var(--font-body);
    font-style: italic;
    font-size: inherit;
    padding: 0;
    cursor: pointer;
    text-decoration: underline;
    text-underline-offset: 3px;
}

.eat-link-btn:hover {
    color: var(--eat-primary-fixed);
}

.fp-back-link {
    font-size: 0.85rem;
}
</style>
