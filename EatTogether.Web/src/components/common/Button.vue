<template>
    <!-- 如果有傳 to，渲染成 RouterLink；否則渲染成 button 或 a -->
    <RouterLink v-if="to" :to="to" :class="buttonClass" v-bind="$attrs">
        <!-- 讀取中 spinner -->
        <span
            v-if="loading"
            class="spinner-border-eat"
            style="width: 0.9rem; height: 0.9rem; border-width: 2px"
        ></span>
        <slot />
    </RouterLink>

    <a
        v-else-if="href"
        :href="href"
        :target="target"
        :rel="target === '_blank' ? 'noopener noreferrer' : undefined"
        :role="loading ? 'status' : undefined"
        :class="buttonClass"
        v-bind="$attrs"
    >
        <span
            v-if="loading"
            class="spinner-border spinner-border-sm spinner-border-eat me-2"
            style="width: 0.9rem; height: 0.9rem; border-width: 2px"
            aria-hidden="true"
        ></span>
        <slot />
    </a>

    <button
        v-else
        :type="type"
        :disabled="disabled || loading"
        :class="buttonClass"
        :role="loading ? 'status' : undefined"
        @click="emit('click', $event)"
        v-bind="$attrs"
    >
        <span
            v-if="loading"
            class="spinner-border spinner-border-sm spinner-border-eat me-2"
            style="width: 0.9rem; height: 0.9rem; border-width: 2px"
            aria-hidden="true"
        ></span>
        <slot />
    </button>
</template>

<script setup>
import { computed } from 'vue'
import { RouterLink } from 'vue-router'

defineOptions({ inheritAttrs: false })

const props = defineProps({
    /**
     * 按鈕樣式變體
     * 'primary' | 'secondary' | 'tertiary' | 'danger' | 'cart'
     */
    variant: {
        type: String,
        default: 'primary',
        validator: (v) => ['primary', 'secondary', 'tertiary', 'danger', 'cart'].includes(v),
    },
    /** RouterLink 的目標路由 */
    to: {
        type: [String, Object],
        default: null,
    },
    /** 一般超連結 href */
    href: {
        type: String,
        default: null,
    },
    /** a 標籤的 target */
    target: {
        type: String,
        default: '_self',
    },
    /** button 的 type */
    type: {
        type: String,
        default: 'button',
    },
    /** 禁用狀態 */
    disabled: {
        type: Boolean,
        default: false,
    },
    /** 讀取中狀態（顯示 spinner） */
    loading: {
        type: Boolean,
        default: false,
    },
})

const emit = defineEmits(['click'])

const buttonClass = computed(() => `btn-eat-${props.variant}`)
</script>

<!-- Button.vue 不需要 <style>。
  所有按鈕樣式（.btn-eat-primary / secondary / tertiary / danger / cart）
  均定義在全域 src/assets/styles/buttons.css，
  spinner 樣式定義在全域 src/assets/styles/animations.css。

  使用範例：
  ───────────────────────────────────────────────────── -->
<!-- 一般按鈕 -->
<!-- <Button variant="primary" @click="handleSubmit">立即訂位</Button>
<Button variant="secondary">查看菜單</Button>
<Button variant="tertiary">瀏覽完整菜單</Button>
<Button variant="danger" @click="handleDelete">刪除</Button>
<Button variant="cart" @click="addToCart">加入購物車</Button> -->

<!-- 讀取中 -->
<!-- <Button variant="primary" :loading="isSubmitting">送出訂位中…</Button> -->

<!-- 禁用 -->
<!-- <Button variant="primary" :disabled="true">已額滿</Button> -->

<!-- RouterLink -->
<!-- <Button variant="primary" to="/reservation">立即訂位</Button> -->

<!-- 一般超連結 -->
<!-- <Button variant="secondary" href="https://..." target="_blank">外部連結</Button> -->
