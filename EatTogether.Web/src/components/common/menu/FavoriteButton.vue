<!-- 愛心收藏按鈕 -->
<template>
    <button
        class="fav-btn"
        :aria-label="isFavorited ? '取消收藏' : '加入收藏'"
        :disabled="disabled"
        @click.stop="emit('toggle', dishId)"
    >
        {{ isFavorited ? '❤️' : '🤍' }}
    </button>
</template>

<script setup>
defineProps({
    dishId: { type: Number, required: true },
    isFavorited: { type: Boolean, default: false },
    disabled: { type: Boolean, default: false },
})

const emit = defineEmits(['toggle'])
</script>

<style scoped>
.fav-btn {
    position: absolute;
    top: 0.75rem;
    right: 0.75rem;
    background: none;
    border: none;
    font-size: 1.1rem;
    line-height: 1;
    cursor: pointer;
    padding: 0;
    filter: drop-shadow(0 1px 3px rgba(0, 0, 0, 0.6));
    transition: transform 0.2s cubic-bezier(0.34, 1.56, 0.64, 1);
}
.fav-btn:hover {
    transform: scale(1.25);
}
.fav-btn:disabled {
    pointer-events: none;
    opacity: 0.4;
}
</style>
