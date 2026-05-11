<template>
    <Transition name="fade">
        <button
            v-if="showBackTop"
            class="back-to-top"
            @click="scrollToTop"
            aria-label="回到頂部"
            title="回到頂部"
        >
            ⤒
        </button>
    </Transition>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'

const props = defineProps({
    threshold: {
        type: Number,
        default: 100,
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
    width: 3.5rem;
    height: 3.5rem;
    border-radius: 50%;
    background: transparent;
    color: var(--eat-secondary);
    border: 1px solid var(--eat-outline-variant);
    cursor: pointer;
    font-size: 1.8rem;
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    transition: var(--eat-transition);
    z-index: 100;
}
.back-to-top:hover {
    background: var(--eat-primary);
    color: var(--eat-on-primary);
    border-color: var(--eat-primary);
    transform: translateY(-3px);
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
