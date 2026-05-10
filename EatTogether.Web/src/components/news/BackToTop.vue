<template>
    <Transition name="fade">
        <button v-if="showBackTop" class="back-to-top" @click="scrollToTop" aria-label="回到頂部">
            ⤒
        </button>
    </Transition>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'

const props = defineProps({
    threshold: {
        type: Number,
        default: 150,
    },
})

const showBackTop = ref(false)

function onScroll() {
    showBackTop.value = window.scrollY > props.threshold
}

function scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' })
}

onMounted(() => window.addEventListener('scroll', onScroll, { passive: true }))
onUnmounted(() => window.removeEventListener('scroll', onScroll))
</script>

<style scoped>
.back-to-top {
    position: fixed;
    bottom: 2rem;
    right: 2rem;
    width: 2.5rem;
    height: 2.5rem;
    border-radius: 50%;
    background: var(--eat-primary);
    color: var(--eat-on-primary, #fff);
    border: none;
    cursor: pointer;
    font-size: 1.1rem;
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    transition:
        opacity 0.2s,
        transform 0.2s;
    z-index: 100;
}
.back-to-top:hover {
    transform: translateY(-3px);
    opacity: 0.85;
}

/* Transition */
.fade-enter-active,
.fade-leave-active {
    transition: opacity 0.3s;
}
.fade-enter-from,
.fade-leave-to {
    opacity: 0;
}
</style>
