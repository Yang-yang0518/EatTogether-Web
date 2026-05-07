<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { Modal } from 'bootstrap'
import { useToast } from '@/composables/useToast.js'
import apiFetch from '@/utils/apiFetch.js'
import Button from '@/components/common/Button.vue'
import FormErrorMessage from '@/components/common/FormErrorMessage.vue'

const emit = defineEmits(['success'])

const { show: showToast } = useToast()

const currentPassword = ref('')
const newPassword = ref('')
const confirmNewPassword = ref('')
const showCurrent = ref(false)
const showNew = ref(false)
const showConfirm = ref(false)
const currentPasswordError = ref('')
const newPasswordError = ref('')
const confirmNewPasswordError = ref('')
const isSubmitting = ref(false)

function resetForm() {
    currentPassword.value = ''
    newPassword.value = ''
    confirmNewPassword.value = ''
    showCurrent.value = false
    showNew.value = false
    showConfirm.value = false
    currentPasswordError.value = ''
    newPasswordError.value = ''
    confirmNewPasswordError.value = ''
    isSubmitting.value = false
}

async function handleSubmit() {
    currentPasswordError.value = ''
    newPasswordError.value = ''
    confirmNewPasswordError.value = ''

    let hasError = false
    if (!currentPassword.value) {
        currentPasswordError.value = '請輸入目前密碼'
        hasError = true
    }
    if (!newPassword.value) {
        newPasswordError.value = '請輸入新密碼'
        hasError = true
    } else if (newPassword.value.length < 8) {
        newPasswordError.value = '新密碼至少 8 個字元'
        hasError = true
    } else if (newPassword.value.length > 128) {
        newPasswordError.value = '新密碼最多 128 個字元'
        hasError = true
    }
    if (!confirmNewPassword.value) {
        confirmNewPasswordError.value = '請確認新密碼'
        hasError = true
    } else if (confirmNewPassword.value !== newPassword.value) {
        confirmNewPasswordError.value = '兩次密碼輸入不一致'
        hasError = true
    }
    if (hasError) return

    isSubmitting.value = true
    try {
        const res = await apiFetch('/members/me/password', {
            method: 'PUT',
            body: JSON.stringify({
                currentPassword: currentPassword.value,
                newPassword: newPassword.value,
            }),
        })

        if (res.ok) {
            Modal.getOrCreateInstance(document.querySelector('#changePasswordModal')).hide()
            showToast('密碼已更新', 'success')
            emit('success')
            return
        }

        if (res.status < 500) {
            const data = await res.json()
            if (data?.errorCode === 'wrong_password') {
                currentPasswordError.value = '舊密碼不正確'
            } else {
                showToast(data?.message || '修改失敗，請稍後再試', 'error')
            }
        }
    } catch (err) {
        if (import.meta.env.DEV) console.error('[ChangePasswordModal.handleSubmit]', err)
    } finally {
        isSubmitting.value = false
    }
}

const handleHidden = () => resetForm()

onMounted(() => {
    const el = document.querySelector('#changePasswordModal')
    el?.addEventListener('hidden.bs.modal', handleHidden)
})

onUnmounted(() => {
    const el = document.querySelector('#changePasswordModal')
    el?.removeEventListener('hidden.bs.modal', handleHidden)
    Modal.getInstance(document.querySelector('#changePasswordModal'))?.dispose()
})
</script>

<template>
    <div id="changePasswordModal" class="modal fade" tabindex="-1">
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
                        <h5 class="eat-h3 fw-bolder fst-normal fs-5 mb-0">修改密碼</h5>
                    </div>
                </div>
                <div class="modal-body px-4 pb-4">
                    <div class="form-eat d-flex flex-column gap-3">
                        <!-- 目前密碼 -->
                        <div>
                            <label class="form-label">目前密碼</label>
                            <div class="position-relative">
                                <input
                                    v-model="currentPassword"
                                    :type="showCurrent ? 'text' : 'password'"
                                    class="form-control"
                                    placeholder="請輸入目前密碼"
                                    autocomplete="current-password"
                                />
                                <button
                                    type="button"
                                    class="btn-eat-password-toggle"
                                    @mousedown.prevent
                                    @click="showCurrent = !showCurrent"
                                    :aria-label="showCurrent ? '隱藏密碼' : '顯示密碼'"
                                >
                                    <i :class="showCurrent ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
                                </button>
                            </div>
                            <FormErrorMessage
                                :message="currentPasswordError"
                                :show="!!currentPasswordError"
                            />
                        </div>
                        <!-- 新密碼 -->
                        <div>
                            <label class="form-label">新密碼</label>
                            <div class="position-relative">
                                <input
                                    v-model="newPassword"
                                    :type="showNew ? 'text' : 'password'"
                                    class="form-control"
                                    placeholder="至少 8 個字元"
                                    autocomplete="new-password"
                                />
                                <button
                                    type="button"
                                    class="btn-eat-password-toggle"
                                    @mousedown.prevent
                                    @click="showNew = !showNew"
                                    :aria-label="showNew ? '隱藏密碼' : '顯示密碼'"
                                >
                                    <i :class="showNew ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
                                </button>
                            </div>
                            <FormErrorMessage
                                :message="newPasswordError"
                                :show="!!newPasswordError"
                            />
                        </div>
                        <!-- 確認新密碼 -->
                        <div>
                            <label class="form-label">確認新密碼</label>
                            <div class="position-relative">
                                <input
                                    v-model="confirmNewPassword"
                                    :type="showConfirm ? 'text' : 'password'"
                                    class="form-control"
                                    placeholder="再次輸入新密碼"
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
                                :message="confirmNewPasswordError"
                                :show="!!confirmNewPasswordError"
                            />
                        </div>
                        <Button
                            variant="primary"
                            class="btn-eat-md mt-2"
                            :loading="isSubmitting"
                            @click="handleSubmit"
                        >
                            {{ isSubmitting ? '修改中...' : '確認修改' }}
                        </Button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
