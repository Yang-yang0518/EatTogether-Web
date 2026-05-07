<template>
    <div
        style="
            min-height: 100vh;
            background-color: var(--eat-surface);
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            gap: 1.5rem;
            text-align: center;
            padding: 2rem;
        "
    >
        <h1 class="eat-h1">404 — 找不到頁面</h1>
        <p style="color: var(--eat-muted)">您造訪的頁面不存在或已移除。</p>
        <button class="btn-eat-primary" @click="router.push('/')">返回首頁</button>
    </div>
</template>

<script setup>
import { useRouter } from 'vue-router'

const router = useRouter()
</script>
