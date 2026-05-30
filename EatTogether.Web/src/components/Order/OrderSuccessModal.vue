<template>
    <Teleport to="body">
        <Transition name="modal-fade">
            <div v-if="visible" class="modal-overlay" @click.self="$emit('close')">
                <div class="dialog">
                    <div class="glow-bg"></div>

                    <div class="checkmark">✓</div>

                    <h2>訂單已送出</h2>
                    <p>您的點餐已送出<br />稍候服務員將為您服務</p>

                    <hr class="divider" />

                    <div class="label">ORDER NUMBER</div>
                    <div class="order-num">{{ orderNumber }}</div>

                    <div class="table-row">
                        桌號 <span>{{ tableLabel }}</span>
                    </div>

                    <button class="btn" @click="$emit('close')">確認</button>
                </div>
            </div>
        </Transition>
    </Teleport>
</template>

<script setup>
defineProps({
    visible: { type: Boolean, default: false },
    orderNumber: { type: String, default: '' },
    tableLabel: { type: String, default: '' },
})
defineEmits(['close'])
</script>

<style scoped>
.modal-overlay {
    position: fixed;
    inset: 0;
    z-index: 9999;
    background: rgba(0, 0, 0, 0.78);
    backdrop-filter: blur(6px);
    display: flex;
    align-items: center;
    justify-content: center;
}

.dialog {
    background: #0d0d0d;
    border: 1px solid #2a2a2a;
    border-radius: 24px;
    padding: 44px 36px;
    width: min(340px, 92vw);
    text-align: center;
    box-shadow:
        0 0 0 1px #1a1a1a,
        0 20px 80px rgba(0, 0, 0, 0.6);
    position: relative;
    overflow: hidden;
}

.glow-bg {
    position: absolute;
    top: -60px;
    left: 50%;
    transform: translateX(-50%);
    width: 200px;
    height: 200px;
    background: radial-gradient(circle, rgba(255, 200, 60, 0.12) 0%, transparent 70%);
    pointer-events: none;
}

.checkmark {
    width: 56px;
    height: 56px;
    border: 2px solid #f5c842;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    margin: 0 auto 24px;
    color: #f5c842;
    font-size: 22px;
    position: relative;
    z-index: 1;
}

h2 {
    font-family: 'Noto Serif TC', serif;
    font-size: 20px;
    color: #ffffff;
    margin: 0 0 8px;
    letter-spacing: 2px;
}

p {
    font-size: 12px;
    color: #555;
    line-height: 1.8;
    margin: 0 0 28px;
    letter-spacing: 0.5px;
}

.divider {
    border: none;
    border-top: 1px solid #1f1f1f;
    margin-bottom: 24px;
}

.label {
    font-size: 10px;
    letter-spacing: 3px;
    color: #444;
    text-transform: uppercase;
    margin-bottom: 8px;
}

.order-num {
    font-size: 22px;
    font-weight: 700;
    color: #f5c842;
    letter-spacing: 2px;
    margin-bottom: 16px;
    font-family: 'Noto Serif TC', serif;
}

.table-row {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    margin-bottom: 28px;
    color: #555;
    font-size: 12px;
}
.table-row span {
    background: #1a1a1a;
    border: 1px solid #2a2a2a;
    padding: 4px 14px;
    border-radius: 6px;
    color: #aaa;
    letter-spacing: 1px;
}

.btn {
    width: 100%;
    padding: 14px;
    background: transparent;
    color: #f5c842;
    border: 1px solid #f5c842;
    border-radius: 10px;
    font-size: 14px;
    font-family: 'Noto Sans TC', sans-serif;
    cursor: pointer;
    letter-spacing: 3px;
    transition:
        background 0.25s,
        color 0.25s;
}
.btn:hover {
    background: #f5c842;
    color: #000;
}
.btn:active {
    transform: scale(0.97);
}

.modal-fade-enter-active,
.modal-fade-leave-active {
    transition:
        opacity 0.25s,
        transform 0.25s;
}
.modal-fade-enter-from,
.modal-fade-leave-to {
    opacity: 0;
    transform: scale(0.95);
}
</style>
