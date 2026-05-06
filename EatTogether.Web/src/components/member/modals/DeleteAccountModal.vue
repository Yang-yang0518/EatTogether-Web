<script setup>
import { ref, onMounted, onUnmounted } from 'vue'

import { Modal } from 'bootstrap'

import { useToast } from '@/composables/useToast.js'
import apiFetch from '@/utils/apiFetch.js'
import Button from '@/components/common/Button.vue'
import FormErrorMessage from '@/components/common/FormErrorMessage.vue'

const props = defineProps({
    hasPassword: { type: Boolean, required: true },
})

const { show: showToast } = useToast()

const password = ref('')
const showPassword = ref(false)
const passwordError = ref('')
const isSubmitting = ref(false)

function resetForm() {
    password.value = ''
    showPassword.value = false
    passwordError.value = ''
    isSubmitting.value = false
}

async function handleSubmit() {
    passwordError.value = ''

    if (props.hasPassword && !password.value) {
        passwordError.value = '請輸入密碼以確認申請'
        return
    }

    isSubmitting.value = true
    try {
        const res = await apiFetch('/members/me/delete-request', {
            method: 'POST',
            body: JSON.stringify({
                password: props.hasPassword ? password.value : null,
            }),
        })

        if (res.ok) {
            Modal.getOrCreateInstance(document.querySelector('#deleteAccountModal')).hide()
            showToast('確認信已寄至您的信箱，請點擊連結完成刪除', 'info')
            return
        }

        if (res.status < 500) {
            const data = await res.json()
            if (data?.errorCode === 'wrong_password') {
                passwordError.value = '密碼不正確'
            } else if (data?.errorCode === 'password_required') {
                passwordError.value = '請輸入密碼'
            } else {
                showToast(data?.message || '申請失敗，請稍後再試', 'error')
            }
        }
    } catch (err) {
        if (import.meta.env.DEV) console.error('[DeleteAccountModal.handleSubmit]', err)
    } finally {
        isSubmitting.value = false
    }
}

const handleHidden = () => resetForm()

onMounted(() => {
    const el = document.querySelector('#deleteAccountModal')
    el?.addEventListener('hidden.bs.modal', handleHidden)
})

onUnmounted(() => {
    const el = document.querySelector('#deleteAccountModal')
    el?.removeEventListener('hidden.bs.modal', handleHidden)
    Modal.getInstance(document.querySelector('#deleteAccountModal'))?.dispose()
})
</script>

<template>
    <div
        id="deleteAccountModal"
        class="modal fade"
        tabindex="-1"
        data-bs-backdrop="static"
        data-bs-keyboard="false"
    >
        <div class="modal-dialog modal-dialog-centered modal-eat-dialog modal-fullscreen-sm-down">
            <div class="modal-content modal-eat-content">
                <button
                    type="button"
                    class="btn-close btn-close-white btn-sm me-2 mt-2 ms-auto"
                    data-bs-dismiss="modal"
                    aria-label="關閉"
                ></button>

                <div class="modal-header border-0 px-4 py-0">
                    <div class="w-100 d-flex justify-content-center">
                        <h5 class="eat-h3 fw-bolder fst-normal fs-5 mb-0">刪除帳號</h5>
                    </div>
                </div>
                <div class="modal-body px-4 pb-4">
                    <div class="d-flex flex-column gap-2">
                        <p class="eat-body-muted mb-1 text-center">
                            將寄送確認信至您的信箱，點擊連結後帳號才會正式停用
                        </p>
                        <!-- HAS_PASSWORD：需密碼確認 -->
                        <div v-if="hasPassword" class="form-eat">
                            <label for="da-modal-password" class="form-label">請輸入密碼確認</label>
                            <div class="position-relative">
                                <input
                                    id="da-modal-password"
                                    v-model="password"
                                    :type="showPassword ? 'text' : 'password'"
                                    class="form-control"
                                    :class="{ 'is-invalid': passwordError }"
                                    placeholder="請輸入目前密碼"
                                    autocomplete="current-password"
                                    :disabled="isSubmitting"
                                    @keyup.enter="handleSubmit"
                                    :aria-describedby="
                                        passwordError ? 'da-modal-password-error' : undefined
                                    "
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
                                id="da-modal-password-error"
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
                            {{ isSubmitting ? '送出中...' : '送出刪除申請' }}
                        </Button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
