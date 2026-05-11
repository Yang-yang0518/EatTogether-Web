<template>
    <Teleport to="body">
        <Transition name="modal-fade">
            <div v-if="visible" class="modal-overlay" @click.self="$emit('close')">
                <div class="dialog">
                    <div class="glow-bg"></div>

                    <!-- 標題列 -->
                    <div class="dialog-header">
                        <h2 class="font-headline">修改取餐資訊</h2>
                        <button class="close-btn" @click="$emit('close')">✕</button>
                    </div>

                    <hr class="divider" />

                    <!-- 表單欄位 -->
                    <div class="fields">
                        <!-- 預計取餐時間 -->
                        <div class="field">
                            <label class="field-label font-label">預計取餐時間</label>
                            <div class="select-wrap">
                                <select v-model="form.pickupTime" class="field-select font-label">
                                    <option v-for="t in pickupTimeOptions" :key="t" :value="t">
                                        今日 {{ t }}
                                    </option>
                                </select>
                                <svg class="select-arrow" xmlns="http://www.w3.org/2000/svg" width="11" height="11" fill="currentColor" viewBox="0 0 16 16">
                                    <path d="M7.247 11.14 2.451 5.658C1.885 5.013 2.345 4 3.204 4h9.592a1 1 0 0 1 .753 1.659l-4.796 5.48a1 1 0 0 1-1.506 0z"/>
                                </svg>
                            </div>
                        </div>

                        <!-- 取餐人 -->
                        <div class="field">
                            <label class="field-label font-label">取餐人</label>
                            <input
                                v-model="form.customerName"
                                class="field-input font-label"
                                placeholder="請輸入取餐人姓名"
                            />
                        </div>

                        <!-- 聯絡電話 -->
                        <div class="field">
                            <label class="field-label font-label">聯絡電話</label>
                            <input
                                v-model="form.customerPhone"
                                class="field-input font-label"
                                placeholder="請輸入聯絡電話"
                                type="tel"
                            />
                        </div>

                        <!-- 餐具 -->
                        <div class="field">
                            <label class="field-label font-label">餐具</label>
                            <div class="toggle-group">
                                <button
                                    :class="['toggle-btn font-label', { active: form.utensils === true }]"
                                    @click="form.utensils = true"
                                >需要</button>
                                <button
                                    :class="['toggle-btn font-label', { active: form.utensils === false }]"
                                    @click="form.utensils = false"
                                >不需要</button>
                            </div>
                        </div>
                    </div>

                    <hr class="divider" />

                    <!-- 操作按鈕 -->
                    <div class="actions">
                        <button class="btn btn-cancel font-label" @click="$emit('close')">取消</button>
                        <button
                            class="btn btn-save font-label"
                            :disabled="saving"
                            @click="handleSave"
                        >
                            <span v-if="saving" class="spinner"></span>
                            {{ saving ? '儲存中…' : '確認修改' }}
                        </button>
                    </div>
                </div>
            </div>
        </Transition>
    </Teleport>
</template>

<script setup>
import { ref, computed, watch } from 'vue'

const props = defineProps({
    visible:   { type: Boolean, default: false },
    order:     { type: Object,  default: null },
    isSaving:  { type: Boolean, default: false },
})

const emit = defineEmits(['close', 'saved'])

// 每 30 分鐘一格，09:00 ～ 21:00
const pickupTimeOptions = (() => {
    const opts = []
    for (let h = 9; h <= 21; h++) {
        opts.push(`${String(h).padStart(2, '0')}:00`)
        if (h < 21) opts.push(`${String(h).padStart(2, '0')}:30`)
    }
    return opts
})()

const saving = computed(() => props.isSaving)
const form = ref({
    pickupTime: '',
    customerName: '',
    customerPhone: '',
    utensils: false,
})

// 每次開啟時帶入訂單現有資料
watch(
    () => props.visible,
    (val) => {
        if (val && props.order) {
            const o = props.order
            const noteRaw = o.note ?? ''
            const noUtensils = /不需要|不要餐具/.test(noteRaw)
            form.value = {
                pickupTime: o.pickupTime ?? pickupTimeOptions[0],
                customerName: o.customerName ?? '',
                customerPhone: o.customerPhone ?? '',
                utensils: !noUtensils,
            }
        }
    }
)

function handleSave() {
    if (saving.value) return
    emit('saved', {
        orderNumber:   props.order?.orderNumber,
        pickupTime:    form.value.pickupTime,
        customerName:  form.value.customerName.trim(),
        customerPhone: form.value.customerPhone.trim(),
        utensils:      form.value.utensils,
    })
}
</script>

<style scoped>
.modal-overlay {
    position: fixed;
    inset: 0;
    z-index: 9999;
    background: rgba(24, 11, 6, 0.88);
    backdrop-filter: blur(6px);
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 1.5rem;
}

.dialog {
    background: #2b1c16;
    border: 1px solid rgba(77, 70, 58, 0.45);
    border-radius: 1rem;
    padding: 2rem;
    width: min(420px, 100%);
    box-shadow: 0 20px 60px rgba(0, 0, 0, 0.55);
    position: relative;
    overflow: hidden;
    display: flex;
    flex-direction: column;
    gap: 0;
}

.glow-bg {
    position: absolute;
    top: -80px;
    left: 50%;
    transform: translateX(-50%);
    width: 260px;
    height: 260px;
    background: radial-gradient(circle, rgba(227, 199, 107, 0.08) 0%, transparent 70%);
    pointer-events: none;
}

/* ── 標題列 ── */
.dialog-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 1.1rem;
    position: relative;
    z-index: 1;
}

h2 {
    font-size: 1.2rem;
    font-style: italic;
    color: #e3c76b;
    margin: 0;
    letter-spacing: 0.06em;
}

.close-btn {
    background: transparent;
    border: none;
    color: rgba(208, 197, 181, 0.35);
    font-size: 0.95rem;
    cursor: pointer;
    padding: 0.2rem 0.4rem;
    border-radius: 0.25rem;
    transition: color 0.2s;
    line-height: 1;
}
.close-btn:hover {
    color: rgba(208, 197, 181, 0.85);
}

.divider {
    border: none;
    border-top: 1px solid rgba(77, 70, 58, 0.4);
    margin: 0 -2rem;
}

/* ── 欄位 ── */
.fields {
    display: flex;
    flex-direction: column;
    gap: 1rem;
    padding: 1.35rem 0;
    position: relative;
    z-index: 1;
}

.field {
    display: flex;
    flex-direction: column;
    gap: 0.4rem;
}

.field-label {
    font-size: 0.78rem;
    letter-spacing: 0.12em;
    color: rgba(208, 197, 181, 0.45);
    text-transform: uppercase;
}

.field-input {
    background: rgba(10, 4, 2, 0.5);
    border: 1px solid rgba(77, 70, 58, 0.5);
    border-radius: 0.45rem;
    color: #f9ddd3;
    font-size: 0.92rem;
    padding: 0.6rem 0.85rem;
    outline: none;
    font-family: 'Work Sans', sans-serif;
    transition: border-color 0.2s;
    width: 100%;
    box-sizing: border-box;
}
.field-input:focus {
    border-color: rgba(227, 199, 107, 0.5);
}
.field-input::placeholder {
    color: rgba(208, 197, 181, 0.25);
}

.select-wrap {
    position: relative;
    display: flex;
    align-items: center;
}
.field-select {
    appearance: none;
    width: 100%;
    background: rgba(10, 4, 2, 0.5);
    border: 1px solid rgba(77, 70, 58, 0.5);
    border-radius: 0.45rem;
    color: #f9ddd3;
    font-size: 0.92rem;
    padding: 0.6rem 2.2rem 0.6rem 0.85rem;
    outline: none;
    font-family: 'Work Sans', sans-serif;
    cursor: pointer;
    transition: border-color 0.2s;
    box-sizing: border-box;
}
.field-select:focus {
    border-color: rgba(227, 199, 107, 0.5);
}
.select-arrow {
    position: absolute;
    right: 0.85rem;
    color: rgba(208, 197, 181, 0.35);
    pointer-events: none;
}

/* ── 餐具切換 ── */
.toggle-group {
    display: flex;
    gap: 0.55rem;
}
.toggle-btn {
    flex: 1;
    padding: 0.6rem;
    background: transparent;
    border: 1px solid rgba(77, 70, 58, 0.5);
    border-radius: 0.45rem;
    color: rgba(208, 197, 181, 0.45);
    font-size: 0.88rem;
    letter-spacing: 0.06em;
    cursor: pointer;
    transition: all 0.2s;
    font-family: 'Work Sans', sans-serif;
}
.toggle-btn:hover {
    border-color: rgba(227, 199, 107, 0.3);
    color: rgba(208, 197, 181, 0.8);
}
.toggle-btn.active {
    border-color: rgba(227, 199, 107, 0.6);
    background: rgba(227, 199, 107, 0.08);
    color: #e3c76b;
}

/* ── 操作按鈕 ── */
.actions {
    display: flex;
    gap: 0.65rem;
    margin-top: 1.25rem;
    position: relative;
    z-index: 1;
}

.btn {
    flex: 1;
    padding: 0.8rem;
    border-radius: 0.5rem;
    font-size: 0.88rem;
    letter-spacing: 0.15em;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.45rem;
    transition: filter 0.2s, transform 0.15s;
    font-family: 'Work Sans', sans-serif;
}
.btn:active:not(:disabled) {
    transform: scale(0.97);
}

.btn-cancel {
    background: transparent;
    border: 1px solid rgba(77, 70, 58, 0.55);
    color: rgba(208, 197, 181, 0.55);
}
.btn-cancel:hover {
    border-color: rgba(208, 197, 181, 0.35);
    color: rgba(208, 197, 181, 0.9);
}

.btn-save {
    flex: 2;
    background: linear-gradient(to right, #e3c76b, #c6ab53);
    border: none;
    color: #3b2f00;
    font-weight: 600;
}
.btn-save:hover:not(:disabled) {
    filter: brightness(1.08);
}
.btn-save:disabled {
    opacity: 0.55;
    cursor: not-allowed;
}

/* ── Spinner ── */
.spinner {
    width: 13px;
    height: 13px;
    border: 2px solid rgba(59, 47, 0, 0.3);
    border-top-color: #3b2f00;
    border-radius: 50%;
    animation: spin 0.7s linear infinite;
    flex-shrink: 0;
}
@keyframes spin {
    to { transform: rotate(360deg); }
}

/* ── Transition ── */
.modal-fade-enter-active,
.modal-fade-leave-active {
    transition: opacity 0.22s, transform 0.22s;
}
.modal-fade-enter-from,
.modal-fade-leave-to {
    opacity: 0;
    transform: scale(0.96);
}
</style>
