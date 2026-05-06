<!-- 分享按鈕 + LINE/FB/X/複製下拉 -->
<template>
    <div class="share-wrap" ref="wrapRef">
        <button class="modal-share" @click.stop="toggle" aria-label="分享">
            <svg
                xmlns="http://www.w3.org/2000/svg"
                width="14"
                height="14"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                stroke-width="2.2"
                stroke-linecap="round"
                stroke-linejoin="round"
            >
                <path d="M4 12v8a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-8" />
                <polyline points="16 6 12 2 8 6" />
                <line x1="12" y1="2" x2="12" y2="15" />
            </svg>
        </button>
        <Transition name="share-menu">
            <div v-if="modelValue" class="share-menu">
                <button class="share-item" @click="emit('select', 'line')">
                    <span class="share-icon si-line">L</span>LINE
                </button>
                <button class="share-item" @click="emit('select', 'facebook')">
                    <span class="share-icon si-fb">f</span>Facebook
                </button>
                <button class="share-item" @click="emit('select', 'x')">
                    <span class="share-icon si-x">𝕏</span>X
                </button>
                <button class="share-item" @click="emit('select', 'copy')">
                    <span class="share-icon si-copy">
                        <svg
                            width="11"
                            height="11"
                            viewBox="0 0 24 24"
                            fill="none"
                            stroke="currentColor"
                            stroke-width="2.5"
                            stroke-linecap="round"
                        >
                            <rect x="9" y="9" width="13" height="13" rx="2" />
                            <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1" />
                        </svg> </span
                    >複製連結
                </button>
            </div>
        </Transition>
    </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'

const props = defineProps({
    modelValue: { type: Boolean, required: true },
    shareUrl: { type: String, required: true },
    shareTitle: { type: String, default: '' },
})

const emit = defineEmits(['update:modelValue', 'select'])

const wrapRef = ref(null)

const toggle = () => emit('update:modelValue', !props.modelValue)

const onClickOutside = (e) => {
    if (props.modelValue && wrapRef.value && !wrapRef.value.contains(e.target))
        emit('update:modelValue', false)
}

onMounted(() => document.addEventListener('click', onClickOutside))
onUnmounted(() => document.removeEventListener('click', onClickOutside))
</script>

<style scoped>
.share-wrap {
    position: relative;
}

.modal-share {
    width: 32px;
    height: 32px;
    border-radius: 50%;
    border: none;
    background: rgba(0, 0, 0, 0.6);
    color: rgba(249, 221, 211, 0.8);
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    backdrop-filter: blur(8px);
}
.modal-share:hover {
    background: rgba(0, 0, 0, 0.8);
    color: var(--eat-primary);
}

.share-menu {
    position: absolute;
    top: calc(100% + 0.45rem);
    right: 0;
    background: rgba(18, 8, 4, 0.96);
    backdrop-filter: blur(16px);
    border: 1px solid rgba(227, 199, 107, 0.15);
    border-radius: 12px;
    padding: 0.4rem;
    display: flex;
    flex-direction: column;
    gap: 0.15rem;
    min-width: 148px;
    box-shadow: 0 12px 40px rgba(0, 0, 0, 0.55);
    z-index: 10;
}
.share-item {
    display: flex;
    align-items: center;
    gap: 0.6rem;
    padding: 0.48rem 0.7rem;
    border: none;
    background: none;
    border-radius: 8px;
    color: rgba(249, 221, 211, 0.75);
    font-size: 0.8rem;
    cursor: pointer;
    transition: background 0.15s;
    white-space: nowrap;
    width: 100%;
    text-align: left;
}
.share-item:hover {
    background: rgba(255, 255, 255, 0.06);
}

.share-icon {
    width: 22px;
    height: 22px;
    border-radius: 6px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 0.72rem;
    font-weight: 700;
    flex-shrink: 0;
    line-height: 1;
}
.si-line {
    background: #06c755;
    color: white;
    border-radius: 50%;
}
.si-fb {
    background: #1877f2;
    color: white;
    border-radius: 50%;
    font-size: 0.88rem;
}
.si-x {
    background: #0f0f0f;
    color: white;
    border-radius: 50%;
    border: 1px solid rgba(255, 255, 255, 0.2);
    font-size: 0.75rem;
}
.si-copy {
    background: rgba(227, 199, 107, 0.12);
    color: var(--eat-primary);
    border: 1px solid rgba(227, 199, 107, 0.3);
    border-radius: 6px;
}

.share-menu-enter-active {
    transition:
        opacity 0.18s ease,
        transform 0.18s cubic-bezier(0.22, 1, 0.36, 1);
}
.share-menu-leave-active {
    transition:
        opacity 0.12s ease,
        transform 0.12s ease;
}
.share-menu-enter-from {
    opacity: 0;
    transform: scale(0.88) translateY(-8px);
    transform-origin: top right;
}
.share-menu-leave-to {
    opacity: 0;
    transform: scale(0.92) translateY(-4px);
    transform-origin: top right;
}
</style>
