<script setup>
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { validateState, clearState, getRedirectPath } from '@/utils/googleOAuth.js'
import { useAuthStore } from '@/stores/auth.js'
import { useToast } from '@/composables/useToast.js'
import apiFetch from '@/utils/apiFetch.js'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'

const router = useRouter()
const authStore = useAuthStore()
const { show } = useToast()

onMounted(async () => {
    const params = new URLSearchParams(window.location.search)
    const code = params.get('code')
    const state = params.get('state')
    const redirectPath = getRedirectPath()

    // state 或 code 不存在，或 state 比對失敗 → 不發 API
    if (!code || !state || !validateState(state)) {
        clearState()
        show('Google 登入失敗，請重試', 'error')
        router.push(redirectPath)
        return
    }

    clearState()

    try {
        const res = await apiFetch('/auth/google/callback', {
            method: 'POST',
            body: JSON.stringify({ code }),
        })

        if (res.ok) {
            await authStore.fetchMe()
            router.push(redirectPath)
            return
        }

        const data = await res.json().catch(() => ({}))
        const messages = {
            account_blacklisted: '帳號已停權，請聯繫客服',
            login_failed: '登入失敗，請聯絡客服',
        }
        show(messages[data.errorCode] || 'Google 登入失敗，請重試', 'error')
        router.push(redirectPath)
    } catch {
        show('Google 登入失敗，請重試', 'error')
        router.push(redirectPath)
    }
})
</script>

<template>
    <LoadingSpinner :fullscreen="true" message="正在完成 Google 登入..." />
</template>
