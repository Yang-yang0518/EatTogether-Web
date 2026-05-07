<script setup>
import Button from '@/components/common/Button.vue'

defineProps({
    birthDate: { type: String, default: '' }, // 已格式化的日期字串，由 Profile 傳入
    isLoading: { type: Boolean, default: false },
})

const emit = defineEmits(['confirm', 'cancel'])
</script>

<template>
    <div
        class="modal fade show d-block"
        tabindex="-1"
        style="background: rgba(0, 0, 0, 0.5)"
        @click.self="emit('cancel')"
    >
        <div class="modal-dialog modal-dialog-centered modal-eat-dialog modal-fullscreen-sm-down">
            <div class="modal-content modal-eat-content">
                <div class="modal-header border-0 px-4 pt-4 pb-0">
                    <div class="w-100 d-flex justify-content-center">
                        <h5 class="eat-h3 fw-bolder fst-normal fs-5 mb-0">確認生日設定</h5>
                    </div>
                </div>
                <div class="modal-body px-4 pb-4 text-center">
                    <p class="eat-body-muted mb-1">您即將將生日設定為</p>
                    <p class="fw-bold fs-5 mb-3">{{ birthDate }}</p>
                    <p class="eat-body-muted mb-4">
                        生日設定後將<span style="color: var(--eat-error)">無法修改</span
                        >，請確認是否正確
                    </p>
                    <div class="d-flex gap-2 justify-content-center">
                        <Button
                            variant="secondary"
                            class="btn-eat-sm"
                            :disabled="isLoading"
                            @click="emit('cancel')"
                        >
                            返回修改
                        </Button>
                        <Button
                            variant="primary"
                            class="btn-eat-sm"
                            :loading="isLoading"
                            @click="emit('confirm')"
                        >
                            {{ isLoading ? '儲存中...' : '確認設定' }}
                        </Button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
