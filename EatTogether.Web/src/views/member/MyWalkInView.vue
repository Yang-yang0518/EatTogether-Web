<template>
    <div>
        <div class="page-header mb-4">
            <h2 class="eat-h3 fst-normal mb-1">我的候位</h2>
            <p class="eat-body-muted mb-0">今日現場候位狀態</p>
        </div>

        <!-- 載入會員資料中 -->
        <div v-if="profileLoading" class="d-flex justify-content-center py-5">
            <LoadingSpinner message="載入中..." />
        </div>

        <!-- 會員沒有電話，引導去個人資料補填 -->
        <div v-else-if="!memberPhone" class="card-eat p-5 text-center">
            <i
                class="bi bi-telephone-x"
                style="font-size: 2.5rem; opacity: 0.35; color: var(--eat-on-surface-variant)"
            ></i>
            <p class="eat-body-muted mt-3 mb-3">
                您的會員資料尚未設定手機號碼，<br />請先前往個人資料補填後再查詢候位。
            </p>
            <Button variant="primary" :to="{ name: 'MemberProfile' }">
                <i class="bi bi-person-gear me-2"></i>前往個人資料
            </Button>
        </div>

        <!-- 有電話：查詢結果 -->
        <template v-else>
            <!-- 查詢中 -->
            <div v-if="loading" class="d-flex justify-content-center py-5">
                <LoadingSpinner message="查詢候位中..." />
            </div>

            <!-- 有候位紀錄 -->
            <template v-else-if="result">
                <div class="walkin-result-card mb-4">
                    <div class="result-label">
                        <i class="bi bi-ticket-perforated me-2"></i>今日候位資訊
                    </div>

                    <div class="result-number">{{ result.queueNumber }}</div>
                    <div class="result-name">
                        {{ result.name }}・{{ result.adultsCount + result.childrenCount }} 人
                    </div>

                    <div class="result-status-row">
                        <span class="status-badge" :class="statusClass(result.status)">
                            <i class="bi me-1" :class="statusIcon(result.status)"></i>
                            {{ result.statusText }}
                        </span>
                    </div>

                    <template v-if="result.status === 0">
                        <div v-if="result.groupsAhead > 0" class="result-ahead">
                            前方還有 <strong>{{ result.groupsAhead }}</strong> 組
                        </div>
                        <div v-else class="result-ahead" style="color: var(--eat-primary)">
                            <i class="bi bi-star me-1"></i>您是下一組！請注意叫號
                        </div>
                    </template>

                    <div class="result-time">登記時間：{{ formatTime(result.registeredAt) }}</div>

                    <div
                        v-if="result.status === 0"
                        class="mt-4 d-flex justify-content-center gap-3"
                    >
                        <Button variant="secondary" @click="leaveQueue" :loading="leaving">
                            <i class="bi bi-x-circle me-1"></i>取消候位
                        </Button>
                        <Button variant="tertiary" @click="fetchStatus" :loading="loading">
                            <i class="bi bi-arrow-clockwise me-1"></i>重新整理
                        </Button>
                    </div>
                </div>
            </template>

            <!-- 查無紀錄 -->
            <div v-else class="card-eat p-5 text-center">
                <i
                    class="bi bi-calendar-x"
                    style="font-size: 2.5rem; opacity: 0.35; color: var(--eat-on-surface-variant)"
                ></i>
                <p class="eat-body-muted mt-3 mb-3">今日尚無候位紀錄</p>
                <Button variant="primary" :to="{ name: 'WalkIn' }">
                    <i class="bi bi-ticket-perforated me-2"></i>前往登記候位
                </Button>
            </div>
        </template>
    </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import apiFetch from '@/utils/apiFetch.js'
import Button from '@/components/common/Button.vue'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'

const profileLoading = ref(true)
const memberPhone = ref('')
const loading = ref(false)
const leaving = ref(false)
const result = ref(null)

// ── 初始化：取會員電話 → 自動查候位 ──────────────────
onMounted(async () => {
    try {
        const res = await apiFetch('/members/me')
        if (res.ok) {
            const data = await res.json()
            memberPhone.value = data.phone ?? ''
        }
    } catch {
        /* 靜默 */
    } finally {
        profileLoading.value = false
    }

    if (memberPhone.value) {
        await fetchStatus()
    }
})

async function fetchStatus() {
    if (!memberPhone.value) return
    loading.value = true
    result.value = null
    try {
        const res = await apiFetch(
            `/WalkInQueues/My?phone=${encodeURIComponent(memberPhone.value)}`
        )
        if (res.ok) result.value = await res.json()
        // 404 = 查無紀錄，result 維持 null
    } catch {
        /* 靜默 */
    } finally {
        loading.value = false
    }
}

async function leaveQueue() {
    if (!result.value || !confirm('確定要取消候位嗎？')) return
    leaving.value = true
    try {
        const res = await apiFetch(`/WalkInQueues/${result.value.id}/Leave`, { method: 'PUT' })
        if (res.ok) {
            result.value = null
        } else {
            const body = await res.json().catch(() => ({}))
            alert(body.message || '取消失敗，請稍後再試')
        }
    } catch {
        alert('取消失敗，請稍後再試')
    } finally {
        leaving.value = false
    }
}

function formatTime(dtStr) {
    const d = new Date(dtStr)
    const pad = (n) => String(n).padStart(2, '0')
    return `${d.getFullYear()}/${pad(d.getMonth() + 1)}/${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

function statusClass(s) {
    return (
        {
            0: 'status-waiting',
            1: 'status-called',
            2: 'status-seated',
            3: 'status-left',
            4: 'status-missed',
        }[s] ?? ''
    )
}

function statusIcon(s) {
    return (
        {
            0: 'bi-hourglass-split',
            1: 'bi-megaphone',
            2: 'bi-check-circle',
            3: 'bi-x-circle',
            4: 'bi-skip-forward',
        }[s] ?? 'bi-question-circle'
    )
}
</script>

<style scoped>
.page-header {
    border-bottom: 1px solid var(--eat-outline-variant);
    padding-bottom: 1rem;
}

/* ── 候位結果卡片 ── */
.walkin-result-card {
    background: linear-gradient(135deg, rgba(41, 24, 17, 0.9) 0%, rgba(55, 32, 18, 0.85) 100%);
    border: 1px solid rgba(227, 199, 107, 0.3);
    border-radius: 1rem;
    padding: 2rem;
    text-align: center;
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
}
.result-label {
    font-size: 0.8rem;
    letter-spacing: 0.1em;
    color: rgba(226, 210, 185, 0.45);
    text-transform: uppercase;
    margin-bottom: 0.75rem;
}
.result-number {
    font-size: 3.5rem;
    font-weight: 800;
    color: var(--eat-primary);
    letter-spacing: 0.1em;
    line-height: 1;
    margin-bottom: 0.4rem;
    text-shadow: 0 0 24px rgba(227, 199, 107, 0.35);
}
.result-name {
    font-size: 1rem;
    color: rgba(226, 210, 185, 0.8);
    margin-bottom: 0.65rem;
}
.result-status-row {
    margin-bottom: 0.65rem;
}
.result-ahead {
    font-size: 0.9rem;
    color: rgba(226, 210, 185, 0.65);
    margin-bottom: 0.4rem;
}
.result-time {
    font-size: 0.78rem;
    color: rgba(226, 210, 185, 0.35);
}

/* ── 狀態標籤 ── */
.status-badge {
    display: inline-flex;
    align-items: center;
    padding: 0.3rem 0.85rem;
    border-radius: 999px;
    font-size: 0.82rem;
    font-weight: 600;
}
.status-waiting {
    background: rgba(227, 199, 107, 0.15);
    color: var(--eat-primary);
    border: 1px solid rgba(227, 199, 107, 0.35);
}
.status-called {
    background: rgba(100, 180, 255, 0.12);
    color: #7bc8ff;
    border: 1px solid rgba(100, 180, 255, 0.3);
}
.status-seated {
    background: rgba(100, 220, 130, 0.12);
    color: #7de0a0;
    border: 1px solid rgba(100, 220, 130, 0.3);
}
.status-left {
    background: rgba(180, 180, 180, 0.1);
    color: rgba(226, 210, 185, 0.5);
    border: 1px solid rgba(180, 180, 180, 0.2);
}
.status-missed {
    background: rgba(224, 90, 90, 0.12);
    color: #e88;
    border: 1px solid rgba(224, 90, 90, 0.3);
}
</style>
