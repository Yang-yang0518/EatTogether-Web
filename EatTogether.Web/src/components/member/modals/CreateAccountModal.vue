<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { Modal } from 'bootstrap'
import { useToast } from '@/composables/useToast.js'
import apiFetch from '@/utils/apiFetch.js'
import Button from '@/components/common/Button.vue'
import FormErrorMessage from '@/components/common/FormErrorMessage.vue'

const emit = defineEmits(['success'])

const { show: showToast } = useToast()

const account = ref('')
const password = ref('')
const confirmPassword = ref('')
const showPassword = ref(false)
const showConfirm = ref(false)
const accountError = ref('')
const passwordError = ref('')
const confirmPasswordError = ref('')
const isSubmitting = ref(false)

function resetForm() {
    account.value = ''
    password.value = ''
    confirmPassword.value = ''
    showPassword.value = false
    showConfirm.value = false
    accountError.value = ''
    passwordError.value = ''
    confirmPasswordError.value = ''
    isSubmitting.value = false
}

async function handleSubmit() {
    accountError.value = ''
    passwordError.value = ''
    confirmPasswordError.value = ''

    let hasError = false
    if (!account.value.trim()) {
        accountError.value = '帳號為必填'
        hasError = true
    } else if (!/^[a-zA-Z0-9_]{3,50}$/.test(account.value)) {
        accountError.value = '帳號限英數字及底線，3–50 字元'
        hasError = true
    }
    if (!password.value) {
        passwordError.value = '密碼為必填'
        hasError = true
    } else if (password.value.length < 8) {
        passwordError.value = '密碼至少 8 個字元'
        hasError = true
    } else if (password.value.length > 128) {
        passwordError.value = '密碼最多 128 個字元'
        hasError = true
    }
    if (!confirmPassword.value) {
        confirmPasswordError.value = '請確認密碼'
        hasError = true
    } else if (confirmPassword.value !== password.value) {
        confirmPasswordError.value = '兩次密碼輸入不一致'
        hasError = true
    }
    if (hasError) return

    isSubmitting.value = true
    try {
        const res = await apiFetch('/members/me/create-account', {
            method: 'POST',
            body: JSON.stringify({
                account: account.value,
                password: password.value,
            }),
        })

        if (res.ok) {
            Modal.getOrCreateInstance(document.querySelector('#createAccountModal')).hide()
            showToast('帳號建立成功', 'success')
            emit('success')
            return
        }

        if (res.status < 500) {
            const data = await res.json()
            if (data?.errorCode === 'account_taken') {
                accountError.value = '帳號已被使用，請換一個'
            } else {
                showToast(data?.message || '建立失敗，請稍後再試', 'error')
            }
        }
    } catch (err) {
        if (import.meta.env.DEV) console.error('[CreateAccountModal.handleSubmit]', err)
    } finally {
        isSubmitting.value = false
    }
}

const handleHidden = () => resetForm()

onMounted(() => {
    const el = document.querySelector('#createAccountModal')
    el?.addEventListener('hidden.bs.modal', handleHidden)
})

onUnmounted(() => {
    const el = document.querySelector('#createAccountModal')
    el?.removeEventListener('hidden.bs.modal', handleHidden)
    Modal.getInstance(document.querySelector('#createAccountModal'))?.dispose()
})
</script>

<template>
    <div id="createAccountModal" class="modal fade" tabindex="-1">
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
                        <h5 class="eat-h3 fw-bolder fst-normal fs-5 mb-0">建立一般帳號</h5>
                    </div>
                </div>
                <div class="modal-body px-4 pb-4">
                    <div class="form-eat d-flex flex-column gap-3">
                        <!-- 帳號 -->
                        <div>
                            <label class="form-label">帳號</label>
                            <input
                                v-model="account"
                                type="text"
                                class="form-control"
                                placeholder="限英數字及底線，3–50 字元"
                                autocomplete="username"
                            />
                            <FormErrorMessage :message="accountError" :show="!!accountError" />
                        </div>
                        <!-- 密碼 -->
                        <div>
                            <label class="form-label">密碼</label>
                            <div class="position-relative">
                                <input
                                    v-model="password"
                                    :type="showPassword ? 'text' : 'password'"
                                    class="form-control"
                                    placeholder="至少 8 個字元"
                                    autocomplete="new-password"
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
                            <FormErrorMessage :message="passwordError" :show="!!passwordError" />
                        </div>
                        <!-- 確認密碼 -->
                        <div>
                            <label class="form-label">確認密碼</label>
                            <div class="position-relative">
                                <input
                                    v-model="confirmPassword"
                                    :type="showConfirm ? 'text' : 'password'"
                                    class="form-control"
                                    placeholder="再次輸入密碼"
                                    autocomplete="new-password"
                                />
                                <button
                                    type="button"
                                    class="btn-eat-password-toggle"
                                    @mousedown.prevent
                                    @click="showConfirm = !showConfirm"
                                    :aria-label="showConfirm ? '隱藏密碼' : '顯示密碼'"
                                >
                                    <i :class="showConfirm ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
                                </button>
                            </div>
                            <FormErrorMessage
                                :message="confirmPasswordError"
                                :show="!!confirmPasswordError"
                            />
                        </div>
                        <Button
                            variant="primary"
                            class="btn-eat-md mt-2"
                            :loading="isSubmitting"
                            @click="handleSubmit"
                        >
                            {{ isSubmitting ? '建立中...' : '建立帳號' }}
                        </Button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
