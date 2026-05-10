const SESSION_KEY = 'oauth_state'
const REDIRECT_KEY = 'oauth_redirect'
const ACTION_KEY = 'oauth_action'

// action 參數新增，預設 'login'，連結時傳入 'link'
export function generateState(redirectPath = '/', action = 'login') {
    const state = crypto.randomUUID()
    sessionStorage.setItem(SESSION_KEY, state)
    sessionStorage.setItem(REDIRECT_KEY, redirectPath)
    sessionStorage.setItem(ACTION_KEY, action)
    return state
}

export function buildGoogleOAuthUrl(state) {
    const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID
    const redirectUri = import.meta.env.VITE_GOOGLE_REDIRECT_URI

    const params = new URLSearchParams({
        client_id: clientId,
        redirect_uri: redirectUri,
        response_type: 'code',
        scope: 'openid email profile',
        state,
        prompt: 'select_account', // 強制每次跳出帳號選擇畫面
    })

    return `https://accounts.google.com/o/oauth2/v2/auth?${params.toString()}`
}

export function validateState(returnedState) {
    const storedState = sessionStorage.getItem(SESSION_KEY)
    if (!storedState || !returnedState) return false
    return storedState === returnedState
}

export function clearState() {
    sessionStorage.removeItem(SESSION_KEY)
    sessionStorage.removeItem(REDIRECT_KEY)
    sessionStorage.removeItem(ACTION_KEY)
}

export function getRedirectPath() {
    return sessionStorage.getItem(REDIRECT_KEY) || '/'
}

// 讀取此次 OAuth 的動作類型
export function getAction() {
    return sessionStorage.getItem(ACTION_KEY) || 'login'
}
