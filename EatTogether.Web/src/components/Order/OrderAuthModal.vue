<template>
    <Teleport to="body">
        <div class="auth-overlay">
            <div class="auth-box">
                <!-- ── 品牌標頭 ── -->
                <div class="auth-brand">
                    <p class="auth-brand-sub font-label">內用掃碼點餐</p>
                </div>

                <!-- ════ 選擇畫面 ════ -->
                <h2 class="auth-title font-headline">歡迎光臨</h2>
                <p class="auth-desc font-body">請選擇點餐方式</p>
                <div class="auth-choices">
                    <button class="choice-btn choice-login font-label" @click="$emit('login')">
                        <svg
                            xmlns="http://www.w3.org/2000/svg"
                            width="18"
                            height="18"
                            fill="currentColor"
                            viewBox="0 0 16 16"
                        >
                            <path
                                d="M8 8a3 3 0 1 0 0-6 3 3 0 0 0 0 6zm2-3a2 2 0 1 1-4 0 2 2 0 0 1 4 0zm4 8c0 1-1 1-1 1H3s-1 0-1-1 1-4 6-4 6 3 6 4zm-1-.004c-.001-.246-.154-.986-.832-1.664C11.516 10.68 10.029 10 8 10c-2.03 0-3.516.68-4.168 1.332-.678.678-.83 1.418-.832 1.664h10z"
                            />
                        </svg>
                        登入會員
                    </button>
                    <button class="choice-btn choice-guest font-label" @click="$emit('guest')">
                        <svg
                            xmlns="http://www.w3.org/2000/svg"
                            width="18"
                            height="18"
                            fill="currentColor"
                            viewBox="0 0 16 16"
                        >
                            <path
                                d="M6 8a3 3 0 1 0 0-6 3 3 0 0 0 0 6zm-5 6s-1 0-1-1 1-4 6-4 6 3 6 4-1 1-1 1H1zM11 3.5a.5.5 0 0 1 .5-.5h4a.5.5 0 0 1 0 1h-4a.5.5 0 0 1-.5-.5zm.5 2.5a.5.5 0 0 0 0 1h4a.5.5 0 0 0 0-1h-4zm2 3a.5.5 0 0 0 0 1h2a.5.5 0 0 0 0-1h-2zm0 3a.5.5 0 0 0 0 1h2a.5.5 0 0 0 0-1h-2z"
                            />
                        </svg>
                        訪客點餐
                    </button>
                </div>
            </div>
        </div>
    </Teleport>
</template>

<script setup>
defineEmits(['login', 'guest'])
</script>

<style scoped>
/* ── Overlay ── */
.auth-overlay {
    position: fixed;
    inset: 0;
    z-index: 9000;
    background: rgba(24, 11, 6, 0.92);
    backdrop-filter: blur(8px);
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 1rem;
}

/* ── Card ── */
.auth-box {
    background: #2b1c16;
    border: 1px solid rgba(77, 70, 58, 0.35);
    border-radius: 1rem;
    padding: 2.5rem 2rem;
    width: min(380px, 100%);
    box-shadow: 0 24px 60px rgba(0, 0, 0, 0.5);
    display: flex;
    flex-direction: column;
    gap: 0;
}

/* ── Brand ── */
.auth-brand {
    text-align: center;
    margin-bottom: 1.5rem;
}
.auth-brand-sub {
    font-size: 0.7rem;
    letter-spacing: 0.25em;
    text-transform: uppercase;
    color: rgba(208, 197, 181, 0.35);
    margin: 0;
}

/* ── Title ── */
.auth-title {
    font-size: 1.6rem;
    font-style: italic;
    color: #e3c76b;
    text-align: center;
    margin: 0 0 0.4rem;
}
.auth-desc {
    font-size: 0.9rem;
    font-style: italic;
    color: rgba(249, 221, 211, 0.45);
    text-align: center;
    margin: 0 0 1.75rem;
}

/* ── Choice buttons ── */
.auth-choices {
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
}
.choice-btn {
    width: 100%;
    padding: 0.9rem 1rem;
    border-radius: 0.5rem;
    font-size: 0.9rem;
    letter-spacing: 0.18em;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.6rem;
    transition:
        filter 0.2s,
        transform 0.15s;
}
.choice-btn:active {
    transform: scale(0.97);
}

.choice-login {
    background: linear-gradient(to right, #e3c76b, #c6ab53);
    color: #3b2f00;
    border: none;
}
.choice-login:hover {
    filter: brightness(1.08);
}

.choice-guest {
    background: transparent;
    color: rgba(208, 197, 181, 0.7);
    border: 1px solid rgba(77, 70, 58, 0.55);
}
.choice-guest:hover {
    border-color: rgba(208, 197, 181, 0.4);
    color: #f9ddd3;
}
</style>
