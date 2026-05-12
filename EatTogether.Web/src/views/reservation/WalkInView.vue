<template>
  <div class="walkin-page">

    <!-- Hero -->
    <header class="walkin-hero">
      <p class="eat-label mb-2" style="color:var(--eat-secondary)">
        <i class="bi bi-people me-2"></i>WALK-IN &amp; TABLE STATUS
      </p>
      <h1 class="eat-h1 fst-normal mb-3 eat-display">現場候位</h1>
      <div class="hero-sub">
        <div class="hero-line"></div>
        <span class="eat-body-muted">
          等待中：<span style="color:var(--eat-primary);font-weight:600">{{ todayStatus.waitingCount }}</span> 組
          &ensp;｜&ensp;
          已叫號：<span style="color:var(--eat-secondary);font-weight:600">{{ todayStatus.calledCount }}</span> 組
        </span>
        <div class="hero-line"></div>
      </div>
    </header>

    <div class="container pb-5">

      <!-- ══════════════════════════════════════════
           即時桌況區塊
      ══════════════════════════════════════════ -->
      <div class="section-title mb-3">
        <i class="bi bi-grid-3x3-gap me-2" style="color:var(--eat-secondary)"></i>
        <span>即時桌況</span>
        <span class="section-time ms-2">{{ currentTimeStr }}</span>
      </div>

      <!-- 載入中 -->
      <div v-if="tableLoading" class="d-flex justify-content-center py-4">
        <LoadingSpinner message="查詢桌況中..." />
      </div>

      <!-- 桌況卡片 -->
      <div v-else-if="tableSlots.length" class="row g-3 mb-2">
        <div v-for="slot in tableSlots" :key="slot.tableType" class="col-6 col-md-3">
          <div class="seat-card" :class="slot.available > 0 ? 'available' : 'full'">
            <div class="seat-badge" :class="slot.available > 0 ? 'badge-open' : 'badge-full'">
              {{ slot.available > 0 ? '有位' : '已滿' }}
            </div>
            <div class="seat-type">{{ slot.tableType }}</div>
            <div class="seat-count">
              <span class="available-num">{{ slot.available }}</span>
              <span class="total-num"> / {{ slot.total }}</span>
            </div>
            <div class="seat-label">剩餘桌數</div>
          </div>
        </div>
      </div>

      <!-- 非營業時間 -->
      <div v-else class="card-eat p-4 text-center mb-2">
        <i class="bi bi-moon-stars" style="font-size:2rem;opacity:.35;color:var(--eat-on-surface-variant)"></i>
        <p class="eat-body-muted mt-2 mb-0">目前非訂位服務時段</p>
      </div>

      <!-- 重新整理 + 訂位按鈕 -->
      <div class="d-flex justify-content-end gap-2 mb-5">
        <Button variant="secondary" class="btn-eat-sm" @click="fetchTableStatus" :loading="tableLoading">
          <i class="bi bi-arrow-clockwise me-1"></i>重新整理
        </Button>
        <Button variant="primary" class="btn-eat-sm" :to="{ name: 'Reservation' }">
          <i class="bi bi-calendar-plus me-1"></i>立即訂位
        </Button>
      </div>

      <!-- 分隔線 -->
      <div class="divider mb-4"></div>

      <!-- ══════════════════════════════════════════
           候位 Tab 切換
      ══════════════════════════════════════════ -->
      <div class="walkin-tabs mb-4">
        <button
          class="walkin-tab"
          :class="{ active: activeTab === 'register' }"
          @click="activeTab = 'register'"
        >
          <i class="bi bi-person-plus me-1"></i>登記候位
        </button>
        <button
          class="walkin-tab"
          :class="{ active: activeTab === 'query' }"
          @click="switchToQuery"
        >
          <i class="bi bi-search me-1"></i>查詢號碼
        </button>
      </div>

      <!-- ═══ 登記候位 Tab ═══ -->
      <Transition name="fade-slide" mode="out-in">
        <div v-if="activeTab === 'register'" key="register">

          <!-- 登記成功：號碼牌 -->
          <div v-if="registered" class="ticket-card mb-4">
            <div class="ticket-header">
              <i class="bi bi-ticket-perforated me-2"></i>候位號碼牌
            </div>
            <div class="ticket-number">{{ registered.queueNumber }}</div>
            <div class="ticket-name">{{ registered.name }}・{{ registered.adultsCount + registered.childrenCount }} 人</div>
            <div class="ticket-status">
              <span class="status-badge status-waiting">
                <i class="bi bi-hourglass-split me-1"></i>{{ registered.statusText }}
              </span>
            </div>
            <div v-if="registered.groupsAhead > 0" class="ticket-ahead">
              前方還有 <strong>{{ registered.groupsAhead }}</strong> 組
            </div>
            <div v-else class="ticket-ahead" style="color:var(--eat-primary)">
              <i class="bi bi-star me-1"></i>您是下一組！請注意叫號
            </div>
            <div class="ticket-time">登記時間：{{ formatTime(registered.registeredAt) }}</div>
            <div class="d-flex justify-content-center gap-3 mt-4">
              <Button variant="secondary" @click="leaveQueue(registered.id)" :loading="leaving">
                <i class="bi bi-x-circle me-1"></i>取消候位
              </Button>
              <Button variant="primary" @click="registered = null">
                <i class="bi bi-plus-circle me-1"></i>重新登記
              </Button>
            </div>
          </div>

          <!-- 候位未開放 -->
          <div v-else-if="!queueStatus.open" class="card-eat p-5 text-center">
            <i :class="`bi ${queueStatus.icon}`" style="font-size:2.5rem;opacity:.35;color:var(--eat-on-surface-variant)"></i>
            <p class="eat-body-muted mt-3 mb-0">{{ queueStatus.message }}</p>
          </div>

          <!-- 登記表單 -->
          <div v-else class="card-eat p-4 p-md-5">
            <h2 class="eat-h3 mb-4">填寫候位資訊</h2>

            <div class="row g-3">
              <!-- 姓名 -->
              <div class="col-12 col-md-6">
                <label class="form-label-eat">姓名 <span class="text-danger">*</span></label>
                <input
                  v-model="form.name"
                  type="text"
                  class="form-control form-control-eat"
                  :class="{ 'is-invalid': errors.name }"
                  placeholder="請輸入姓名"
                  maxlength="50"
                />
                <div v-if="errors.name" class="invalid-feedback d-block">{{ errors.name }}</div>
              </div>

              <!-- 電話 -->
              <div class="col-12 col-md-6">
                <label class="form-label-eat">手機號碼 <span class="text-danger">*</span></label>
                <input
                  v-model="form.phone"
                  type="tel"
                  class="form-control form-control-eat"
                  :class="{ 'is-invalid': errors.phone }"
                  placeholder="09xxxxxxxx"
                  maxlength="10"
                />
                <div v-if="errors.phone" class="invalid-feedback d-block">{{ errors.phone }}</div>
              </div>

              <!-- 大人人數 -->
              <div class="col-6 col-md-3">
                <label class="form-label-eat">大人人數 <span class="text-danger">*</span></label>
                <select
                  v-model="form.adultsCount"
                  class="form-select form-select-eat"
                  :class="{ 'is-invalid': errors.people }"
                >
                  <option v-for="n in 10" :key="n" :value="n">{{ n }} 位</option>
                </select>
              </div>

              <!-- 小孩人數 -->
              <div class="col-6 col-md-3">
                <label class="form-label-eat">兒童人數</label>
                <select v-model="form.childrenCount" class="form-select form-select-eat">
                  <option :value="0">0 位</option>
                  <option v-for="n in 10" :key="n" :value="n">{{ n }} 位</option>
                </select>
              </div>

              <!-- 備註 -->
              <div class="col-12">
                <label class="form-label-eat">備註（選填）</label>
                <input
                  v-model="form.remark"
                  type="text"
                  class="form-control form-control-eat"
                  placeholder="如：需嬰兒椅、過敏食材…"
                  maxlength="200"
                />
              </div>

              <!-- 人數錯誤 -->
              <div v-if="errors.people" class="col-12">
                <div class="text-danger small">{{ errors.people }}</div>
              </div>
            </div>

            <!-- 送出錯誤 -->
            <div v-if="submitError" class="alert-eat alert-eat--error mt-3">
              <i class="bi bi-exclamation-circle me-2"></i>{{ submitError }}
            </div>

            <div class="d-flex justify-content-center mt-4">
              <Button variant="primary" @click="submitRegister" :loading="submitting">
                <i class="bi bi-ticket-perforated me-2"></i>確認登記候位
              </Button>
            </div>
          </div>

        </div>
      </Transition>

      <!-- ═══ 查詢號碼 Tab ═══ -->
      <Transition name="fade-slide" mode="out-in">
        <div v-if="activeTab === 'query'" key="query">
          <div class="card-eat p-4 p-md-5">
            <h2 class="eat-h3 mb-4">查詢候位狀態</h2>

            <div class="row g-3 align-items-end">
              <div class="col-12 col-md-8">
                <label class="form-label-eat">手機號碼</label>
                <input
                  v-model="queryPhone"
                  type="tel"
                  class="form-control form-control-eat"
                  :class="{ 'is-invalid': queryError }"
                  placeholder="09xxxxxxxx"
                  maxlength="10"
                  @keyup.enter="submitQuery"
                />
                <div v-if="queryError" class="invalid-feedback d-block">{{ queryError }}</div>
              </div>
              <div class="col-12 col-md-4">
                <Button variant="primary" class="w-100" @click="submitQuery" :loading="querying">
                  <i class="bi bi-search me-1"></i>查詢
                </Button>
              </div>
            </div>

            <!-- 查詢結果 -->
            <Transition name="fade-slide">
              <div v-if="queryResult" class="mt-4">
                <div class="ticket-card">
                  <div class="ticket-header">
                    <i class="bi bi-ticket-perforated me-2"></i>您的候位資訊
                  </div>
                  <div class="ticket-number">{{ queryResult.queueNumber }}</div>
                  <div class="ticket-name">{{ queryResult.name }}・{{ queryResult.adultsCount + queryResult.childrenCount }} 人</div>
                  <div class="ticket-status">
                    <span class="status-badge" :class="statusClass(queryResult.status)">
                      <i class="bi me-1" :class="statusIcon(queryResult.status)"></i>
                      {{ queryResult.statusText }}
                    </span>
                  </div>
                  <template v-if="queryResult.status === 0">
                    <div v-if="queryResult.groupsAhead > 0" class="ticket-ahead">
                      前方還有 <strong>{{ queryResult.groupsAhead }}</strong> 組
                    </div>
                    <div v-else class="ticket-ahead" style="color:var(--eat-primary)">
                      <i class="bi bi-star me-1"></i>您是下一組！請注意叫號
                    </div>
                  </template>
                  <div class="ticket-time">登記時間：{{ formatTime(queryResult.registeredAt) }}</div>
                  <div v-if="queryResult.status === 0" class="d-flex justify-content-center mt-4">
                    <Button variant="secondary" @click="leaveQueue(queryResult.id)" :loading="leaving">
                      <i class="bi bi-x-circle me-1"></i>取消候位
                    </Button>
                  </div>
                </div>
              </div>
            </Transition>
          </div>
        </div>
      </Transition>

    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, onUnmounted } from 'vue'
import apiFetch from '@/utils/apiFetch.js'
import Button from '@/components/common/Button.vue'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import { useAuthStore } from '@/stores/auth.js'
import { useToast } from '@/composables/useToast.js'

const authStore = useAuthStore()
const { show } = useToast()

// ── 時間顯示 ──────────────────────────────────────
const currentTimeStr = ref('')

function updateTime() {
  const now = new Date()
  const pad = n => String(n).padStart(2, '0')
  currentTimeStr.value = `${now.getFullYear()}/${pad(now.getMonth()+1)}/${pad(now.getDate())} ${pad(now.getHours())}:${pad(now.getMinutes())}`
  return now
}

// ── 即時桌況 ──────────────────────────────────────
const tableSlots   = ref([])
const tableLoading = ref(false)

async function fetchTableStatus() {
  tableLoading.value = true
  updateTime()
  try {
    const res = await apiFetch('/Tables')
    if (res.ok) {
      const tables = await res.json()
      // 依座位數分組，計算各類型空桌 / 總桌數
      const groups = {}
      for (const t of tables) {
        const key = `${t.seatCount} 人桌`
        if (!groups[key]) groups[key] = { tableType: key, total: 0, available: 0 }
        groups[key].total++
        if (t.status === 0) groups[key].available++  // 0 = 空桌
      }
      // 依座位數排序
      tableSlots.value = Object.values(groups).sort((a, b) =>
        parseInt(a.tableType) - parseInt(b.tableType)
      )
    }
  } catch { /* 靜默 */ }
  finally { tableLoading.value = false }
}

// ── 今日候位摘要 ──────────────────────────────────
const todayStatus = ref({ waitingCount: 0, calledCount: 0 })

async function fetchTodayStatus() {
  try {
    const res = await apiFetch('/WalkInQueues/TodayStatus')
    if (res.ok) todayStatus.value = await res.json()
  } catch { /* 靜默 */ }
}

// ── 定時刷新 ─────────────────────────────────────
let refreshTimer = null
const currentTime = ref(new Date())   // ← 供 queueStatus computed 使用

onMounted(async () => {
  fetchTableStatus()
  fetchTodayStatus()
  refreshTimer = setInterval(() => {
    fetchTableStatus()
    fetchTodayStatus()
    currentTime.value = new Date()    // ← 每分鐘更新，觸發 queueStatus 重算
  }, 60_000)

  // 登入狀態：自動填入候位表單
  if (authStore.isLoggedIn) {
    try {
      const res = await apiFetch('/members/me')
      if (res.ok) {
        const data = await res.json()
        if (data.name)  form.name  = data.name
        if (data.phone) form.phone = data.phone
      }
    } catch { /* 靜默 */ }
  }
})
onUnmounted(() => clearInterval(refreshTimer))

// ── 候位開放時間判斷 ──────────────────────────────
const QUEUE_OPEN_HOUR   = 10
const QUEUE_OPEN_MIN    = 30
const QUEUE_CLOSE_HOUR  = 21
const QUEUE_CLOSE_MIN   = 0

const queueStatus = computed(() => {
  const now = currentTime.value        // ← 改用 reactive ref，computed 才會重算
  const h = now.getHours()
  const m = now.getMinutes()
  const totalMin = h * 60 + m
  const openMin  = QUEUE_OPEN_HOUR  * 60 + QUEUE_OPEN_MIN
  const closeMin = QUEUE_CLOSE_HOUR * 60 + QUEUE_CLOSE_MIN

  if (totalMin < openMin) {
    const pad = n => String(n).padStart(2, '0')
    return {
      open: false,
      message: `候位將於 ${pad(QUEUE_OPEN_HOUR)}:${pad(QUEUE_OPEN_MIN)} 開放`,
      icon: 'bi-clock'
    }
  }
  if (totalMin >= closeMin) {
    return { open: false, message: '今日候位已結束，歡迎明日再來', icon: 'bi-moon-stars' }
  }
  return { open: true, message: '', icon: '' }
})

// ── Tab 切換 ──────────────────────────────────────
const activeTab = ref('register')

function switchToQuery() {
  activeTab.value = 'query'
  queryPhone.value = ''
  queryError.value = ''
  queryResult.value = null
}

// ── 登記候位 ──────────────────────────────────────
const form = reactive({
  name: '',
  phone: '',
  adultsCount: 2,
  childrenCount: 0,
  remark: '',
})
const errors     = reactive({ name: '', phone: '', people: '' })
const submitting = ref(false)
const submitError = ref('')
const registered = ref(null)

function validateRegister() {
  errors.name = ''
  errors.phone = ''
  errors.people = ''
  let valid = true

  if (!form.name || form.name.length < 2 || form.name.length > 50) {
    errors.name = '請輸入正確姓名（2～50 字）'
    valid = false
  }
  if (!form.phone || !/^09\d{8}$/.test(form.phone)) {
    errors.phone = '請輸入有效手機號碼（格式：09xxxxxxxx）'
    valid = false
  }
  const total = form.adultsCount + form.childrenCount
  if (total < 1 || total > 10) {
    errors.people = '候位人數須在 1～10 人之間'
    valid = false
  }
  return valid
}

async function submitRegister() {
  submitError.value = ''
  if (!validateRegister()) return

  submitting.value = true
  try {
    const res = await apiFetch('/WalkInQueues', {
      method: 'POST',
      body: JSON.stringify({
        name:          form.name.trim(),
        phone:         form.phone.trim(),
        adultsCount:   form.adultsCount,
        childrenCount: form.childrenCount,
        remark:        form.remark.trim(),
      }),
    })
    if (res.ok) {
      registered.value = await res.json()
      await fetchTodayStatus()
    } else {
      const body = await res.json().catch(() => ({}))
      submitError.value = body.message || '登記失敗，請稍後再試'
    }
  } catch {
    submitError.value = '登記失敗，請稍後再試'
  } finally {
    submitting.value = false
  }
}

// ── 取消候位 ─────────────────────────────────────
const leaving = ref(false)

async function leaveQueue(id) {
  if (!confirm('確定要取消候位嗎？')) return
  leaving.value = true
  try {
    const res = await apiFetch(`/WalkInQueues/${id}/Leave`, { method: 'PUT' })
    if (res.ok) {
      registered.value  = null
      queryResult.value = null
      show('已取消候位', 'success')
      await fetchTodayStatus()
    } else {
      const body = await res.json().catch(() => ({}))
      show(body.message || '取消失敗，請稍後再試', 'error')
    }
  } catch {
    show('取消失敗，請稍後再試', 'error')
  } finally {
    leaving.value = false
  }
}

// ── 查詢候位 ─────────────────────────────────────
const queryPhone  = ref('')
const queryError  = ref('')
const querying    = ref(false)
const queryResult = ref(null)

async function submitQuery() {
  queryError.value  = ''
  queryResult.value = null

  if (!queryPhone.value || !/^09\d{8}$/.test(queryPhone.value)) {
    queryError.value = '請輸入有效手機號碼（格式：09xxxxxxxx）'
    return
  }

  querying.value = true
  try {
    const res = await apiFetch(`/WalkInQueues/My?phone=${encodeURIComponent(queryPhone.value)}`)
    if (res.ok) {
      queryResult.value = await res.json()
    } else {
      const body = await res.json().catch(() => ({}))
      queryError.value = body.message || '查無今日候位紀錄'
    }
  } catch {
    queryError.value = '查詢失敗，請稍後再試'
  } finally {
    querying.value = false
  }
}

// ── 工具函式 ─────────────────────────────────────
function formatTime(dtStr) {
  const d = new Date(dtStr)
  const pad = n => String(n).padStart(2, '0')
  return `${d.getFullYear()}/${pad(d.getMonth()+1)}/${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

function statusClass(status) {
  return { 0: 'status-waiting', 1: 'status-called', 2: 'status-seated', 3: 'status-left', 4: 'status-missed' }[status] ?? ''
}

function statusIcon(status) {
  return { 0: 'bi-hourglass-split', 1: 'bi-megaphone', 2: 'bi-check-circle', 3: 'bi-x-circle', 4: 'bi-skip-forward' }[status] ?? 'bi-question-circle'
}
</script>

<style scoped>
/* ── 頁面佈局 ── */
.walkin-page {
  min-height: 100vh;
  background: var(--eat-bg);
}

/* ── Hero ── */
.walkin-hero {
  padding: 5rem 2rem 4rem;
  text-align: center;
  background-color: var(--eat-surface-container);
  border-bottom: 1px solid var(--eat-outline-variant);
  margin-bottom: 2.5rem;
}
.hero-sub {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1.25rem;
  opacity: 0.85;
}
.hero-line {
  flex: 0 0 3rem;
  height: 1px;
  background: var(--eat-secondary);
  opacity: 0.3;
}

/* ── 區塊標題 ── */
.section-title {
  display: flex;
  align-items: center;
  font-family: var(--font-label);
  font-size: .8rem;
  letter-spacing: .1em;
  text-transform: uppercase;
  color: rgba(226,210,185,.55);
}
.section-time {
  font-size: .75rem;
  color: rgba(226,210,185,.35);
}

/* ── 桌況卡片（從 TableStatusView 搬移）── */
.seat-card {
  position: relative;
  border-radius: var(--eat-radius-lg);
  padding: 1.5rem 1rem;
  text-align: center;
  border: 1px solid rgba(180,120,30,.2);
  background: var(--eat-surface-container);
  transition: transform .2s, box-shadow .2s;
}
.seat-card:hover { transform: translateY(-3px); box-shadow: 0 12px 32px rgba(0,0,0,.4); }
.seat-card.available { border-color: rgba(60,179,113,.5); }
.seat-card.full      { border-color: rgba(192,57,43,.4); opacity: .75; }

.seat-badge {
  position: absolute;
  top: 10px; right: 10px;
  font-family: var(--font-label);
  font-size: .6rem;
  letter-spacing: .1em;
  padding: .15rem .5rem;
  border-radius: 50px;
}
.badge-open { background: rgba(60,179,113,.2);  color: #3cb371; border: 1px solid rgba(60,179,113,.4); }
.badge-full { background: rgba(192,57,43,.2);   color: #e05a50; border: 1px solid rgba(192,57,43,.4); }

.seat-type {
  font-family: var(--font-label);
  font-size: .8rem;
  letter-spacing: .08em;
  color: var(--eat-on-surface-variant);
  margin-bottom: .5rem;
}
.seat-count { line-height: 1; margin-bottom: .4rem; }
.available-num { font-family: var(--font-headline); font-size: 2.2rem; font-weight: 700; color: var(--eat-primary); }
.total-num     { font-size: 1rem; color: var(--eat-on-surface-variant); }
.seat-label    { font-size: .72rem; color: var(--eat-on-surface-variant); opacity: .7; }

/* ── 分隔線 ── */
.divider {
  border: none;
  border-top: 1px solid rgba(180,120,30,.15);
}

/* ── Tabs ── */
.walkin-tabs {
  display: flex;
  gap: .5rem;
  border-bottom: 1px solid rgba(180,120,30,.2);
}
.walkin-tab {
  background: none;
  border: none;
  border-bottom: 2px solid transparent;
  color: rgba(226,210,185,.6);
  font-family: var(--font-body);
  font-size: 1rem;
  padding: .6rem 1.25rem;
  cursor: pointer;
  transition: color .2s, border-color .2s;
  margin-bottom: -1px;
}
.walkin-tab:hover  { color: rgba(226,210,185,.9); }
.walkin-tab.active { color: var(--eat-primary); border-bottom-color: var(--eat-primary); font-weight: 600; }

/* ── 表單 ── */
.form-label-eat {
  display: block;
  font-size: .875rem;
  color: rgba(226,210,185,.75);
  margin-bottom: .35rem;
  letter-spacing: .04em;
}
.form-control-eat,
.form-select-eat {
  background: rgba(255,255,255,.05);
  border: 1px solid rgba(180,120,30,.25);
  border-radius: .375rem;
  color: var(--eat-on-surface);
  padding: .6rem .85rem;
  width: 100%;
  font-size: .95rem;
  transition: border-color .2s, box-shadow .2s;
}
.form-control-eat::placeholder { color: rgba(226,210,185,.35); }
.form-control-eat:focus,
.form-select-eat:focus {
  outline: none;
  border-color: rgba(227,199,107,.55);
  box-shadow: 0 0 0 3px rgba(227,199,107,.12);
}
.form-control-eat.is-invalid,
.form-select-eat.is-invalid { border-color: #e05a5a; }
.invalid-feedback { color: #e05a5a; font-size: .8rem; margin-top: .25rem; }
.form-select-eat option { background: #1e120b; color: var(--eat-on-surface); }

/* ── 錯誤提示框 ── */
.alert-eat {
  padding: .75rem 1rem;
  border-radius: .375rem;
  font-size: .9rem;
}
.alert-eat--error {
  background: rgba(224,90,90,.12);
  border: 1px solid rgba(224,90,90,.3);
  color: #e88;
}

/* ── 號碼牌卡片 ── */
.ticket-card {
  background: linear-gradient(135deg, rgba(41,24,17,.9) 0%, rgba(55,32,18,.85) 100%);
  border: 1px solid rgba(227,199,107,.3);
  border-radius: 1rem;
  padding: 2rem;
  text-align: center;
  box-shadow: 0 8px 32px rgba(0,0,0,.35), inset 0 1px 0 rgba(255,255,255,.05);
}
.ticket-header {
  font-size: .85rem;
  letter-spacing: .1em;
  color: rgba(226,210,185,.55);
  text-transform: uppercase;
  margin-bottom: .75rem;
}
.ticket-number {
  font-size: 4rem;
  font-weight: 800;
  font-family: var(--font-display, monospace);
  color: var(--eat-primary);
  line-height: 1;
  letter-spacing: .1em;
  margin-bottom: .5rem;
  text-shadow: 0 0 30px rgba(227,199,107,.4);
}
.ticket-name   { font-size: 1.1rem; color: rgba(226,210,185,.85); margin-bottom: .75rem; }
.ticket-status { margin-bottom: .75rem; }
.ticket-ahead  { font-size: .95rem; color: rgba(226,210,185,.7); margin-bottom: .5rem; }
.ticket-time   { font-size: .8rem; color: rgba(226,210,185,.4); }

/* ── 狀態標籤 ── */
.status-badge {
  display: inline-flex;
  align-items: center;
  padding: .3rem .85rem;
  border-radius: 999px;
  font-size: .85rem;
  font-weight: 600;
}
.status-waiting { background: rgba(227,199,107,.15); color: var(--eat-primary);          border: 1px solid rgba(227,199,107,.35); }
.status-called  { background: rgba(100,180,255,.12); color: #7bc8ff;                     border: 1px solid rgba(100,180,255,.3);  }
.status-seated  { background: rgba(100,220,130,.12); color: #7de0a0;                     border: 1px solid rgba(100,220,130,.3);  }
.status-left    { background: rgba(180,180,180,.1);  color: rgba(226,210,185,.5);        border: 1px solid rgba(180,180,180,.2);  }
.status-missed  { background: rgba(224,90,90,.12);   color: #e88;                        border: 1px solid rgba(224,90,90,.3);    }

/* ── 動畫 ── */
.fade-slide-enter-active,
.fade-slide-leave-active { transition: opacity .2s ease, transform .2s ease; }
.fade-slide-enter-from   { opacity: 0; transform: translateY(8px);  }
.fade-slide-leave-to     { opacity: 0; transform: translateY(-8px); }
</style>
