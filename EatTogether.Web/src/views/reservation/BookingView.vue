<template>
  <div class="booking-page">

    <!-- ── Hero ── -->
    <header class="booking-hero">
      <p class="eat-label mb-2" style="color:var(--eat-secondary)">
        <i class="bi bi-calendar-check me-2"></i>ONLINE RESERVATION
      </p>
      <h1 class="eat-h1 fst-normal mb-3 eat-display">線上訂位</h1>
      <div class="hero-sub">
        <div class="hero-line"></div>
        <span class="eat-body-muted">即時確認桌位，輕鬆安排您的義式饗宴</span>
        <div class="hero-line"></div>
      </div>
      <div class="mt-4">
        <router-link to="/reservation/query" class="query-link">
          <i class="bi bi-search me-2"></i>查詢 / 取消訂位
        </router-link>
      </div>
    </header>

    <div class="container pb-5">
      <div class="form-wrapper">
        <form @submit.prevent="submitBooking" novalidate>
          <div class="row g-4">

            <!-- ── 日期 ── -->
            <div class="col-md-6">
              <label class="form-label-eat">訂位日期 <span class="required-star">*</span></label>
              <input
                v-model="form.date"
                type="date"
                class="form-control-eat"
                :class="{ 'is-invalid': errors.date }"
                :min="minDateStr"
                :max="maxDateStr"
                @change="onDateTimeChange"
              />
              <div v-if="errors.date" class="field-error">{{ errors.date }}</div>
            </div>

            <!-- ── 時段 ── -->
            <div class="col-md-6">
              <label class="form-label-eat">訂位時段 <span class="required-star">*</span></label>
              <div class="d-flex gap-2">
                <select
                  v-model="form.hour"
                  class="form-select-eat"
                  :class="{ 'is-invalid': errors.time }"
                  @change="onDateTimeChange"
                >
                  <option value="">時</option>
                  <option v-for="h in hours" :key="h" :value="h">{{ String(h).padStart(2,'0') }} 時</option>
                </select>
                <select
                  v-model="form.minute"
                  class="form-select-eat"
                  :class="{ 'is-invalid': errors.time }"
                  @change="onDateTimeChange"
                >
                  <option value="">分</option>
                  <option v-for="m in minutes" :key="m" :value="m">{{ String(m).padStart(2,'0') }} 分</option>
                </select>
              </div>
              <div v-if="errors.time" class="field-error">{{ errors.time }}</div>
            </div>

            <!-- ── 大人人數 ── -->
            <div class="col-md-6">
              <label class="form-label-eat">大人人數 <span class="required-star">*</span></label>
              <div class="number-input">
                <button type="button" class="num-btn" @click="form.adults = Math.max(1, form.adults - 1); onDateTimeChange()">
                  <i class="bi bi-dash"></i>
                </button>
                <span class="num-value">{{ form.adults }}</span>
                <button type="button" class="num-btn" @click="form.adults = Math.min(10, form.adults + 1); onDateTimeChange()">
                  <i class="bi bi-plus"></i>
                </button>
              </div>
              <div v-if="errors.people" class="field-error">{{ errors.people }}</div>
            </div>

            <!-- ── 小孩人數 ── -->
            <div class="col-md-6">
              <label class="form-label-eat">兒童人數</label>
              <div class="number-input">
                <button type="button" class="num-btn" @click="form.children = Math.max(0, form.children - 1); onDateTimeChange()">
                  <i class="bi bi-dash"></i>
                </button>
                <span class="num-value">{{ form.children }}</span>
                <button type="button" class="num-btn" @click="form.children = Math.min(10, form.children + 1); onDateTimeChange()">
                  <i class="bi bi-plus"></i>
                </button>
              </div>
            </div>

            <!-- ── 即時桌況回饋 ── -->
            <div v-if="availability" class="col-12">
              <div class="availability-feedback" :class="availability.isAvailable ? 'available' : 'unavailable'">
                <i class="bi me-2" :class="availability.isAvailable ? 'bi-check-circle-fill' : 'bi-x-circle-fill'"></i>
                {{ availability.message }}
              </div>
            </div>

            <!-- ── 分隔線 ── -->
            <div class="col-12">
              <div class="form-divider"></div>
            </div>

            <!-- ── 姓名 ── -->
            <div class="col-md-6">
              <label class="form-label-eat">姓名 <span class="required-star">*</span></label>
              <input
                v-model.trim="form.name"
                type="text"
                class="form-control-eat"
                :class="{ 'is-invalid': errors.name }"
                placeholder="請輸入姓名"
              />
              <div v-if="errors.name" class="field-error">{{ errors.name }}</div>
            </div>

            <!-- ── 電話 ── -->
            <div class="col-md-6">
              <label class="form-label-eat">手機號碼 <span class="required-star">*</span></label>
              <input
                v-model.trim="form.phone"
                type="tel"
                class="form-control-eat"
                :class="{ 'is-invalid': errors.phone }"
                placeholder="09xxxxxxxx"
              />
              <div v-if="errors.phone" class="field-error">{{ errors.phone }}</div>
            </div>

            <!-- ── Email ── -->
            <div class="col-12">
              <label class="form-label-eat">
                Email
                <span class="label-hint">（選填，用於接收確認信）</span>
              </label>
              <input
                v-model.trim="form.email"
                type="email"
                class="form-control-eat"
                :class="{ 'is-invalid': errors.email }"
                placeholder="example@email.com"
              />
              <div v-if="errors.email" class="field-error">{{ errors.email }}</div>
            </div>

            <!-- ── 備註 ── -->
            <div class="col-12">
              <label class="form-label-eat">
                備註
                <span class="label-hint">（過敏食材、特殊需求等）</span>
              </label>
              <textarea
                v-model.trim="form.remark"
                class="form-control-eat"
                rows="3"
                placeholder="如：素食、嬰兒椅、生日布置需求…"
              ></textarea>
            </div>

            <!-- ── 送出 ── -->
            <div class="col-12 text-center pt-2">
              <button
                type="submit"
                class="submit-btn"
                :disabled="submitting || (availability && !availability.isAvailable)"
              >
                <span v-if="submitting" class="spinner-border spinner-border-sm me-2" style="width:.9rem;height:.9rem;border-width:2px"></span>
                <i v-else class="bi bi-calendar-check me-2"></i>
                {{ submitting ? '訂位中…' : '確認訂位' }}
              </button>
            </div>

          </div>
        </form>
      </div>
    </div>

    <!-- ── 成功 Modal ── -->
    <Transition name="modal-fade">
      <div v-if="successModal" class="modal-overlay" @click.self="successModal = false">
        <div class="success-modal">
          <div class="success-glow"></div>
          <div class="success-icon-wrap">
            <i class="bi bi-calendar-check-fill"></i>
          </div>
          <h3 class="success-title">訂位成功！</h3>
          <p class="success-sub">感謝您的訂位，我們期待您的到來</p>
          <div class="booking-number-card">
            <div class="bn-label">訂位單號</div>
            <div class="bn-value">{{ bookingNumber }}</div>
          </div>
          <p class="success-hint">請記錄訂位單號，或至「查詢訂位」隨時查看狀態</p>
          <div class="d-flex gap-3 justify-content-center mt-4">
            <router-link to="/reservation/query" class="btn-ghost-eat">
              <i class="bi bi-search me-1"></i>查詢訂位
            </router-link>
            <button class="btn-primary-eat" @click="successModal = false">
              關閉
            </button>
          </div>
        </div>
      </div>
    </Transition>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import apiFetch from '@/utils/apiFetch.js'
import { useToast } from '@/composables/useToast.js'
import { useAuthStore } from '@/stores/auth.js'

const { show }  = useToast()
const authStore = useAuthStore()

// 時段選項
const hours   = Array.from({ length: 10 }, (_, i) => i + 11)  // 11~20
const minutes = [0, 15, 30, 45]

// 日期範圍（原生 input[type=date] 需要 "yyyy-MM-dd" 格式）
function toDateStr(d) {
  const pad = n => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`
}
const minDateStr = computed(() => {
  const d = new Date()
  d.setMinutes(d.getMinutes() + 30)
  return toDateStr(d)
})
const maxDateStr = computed(() => {
  const d = new Date()
  d.setDate(d.getDate() + 90)
  return toDateStr(d)
})

// 預設：今日日期 + 最早可訂時段（當下 +30 分鐘，進位至 15 分鐘刻度，限 11:00～20:00）
function getDefaultDateTime() {
  const earliest = new Date(Date.now() + 30 * 60_000)

  // 進位至下一個 15 分鐘刻度
  const roundUp = Math.ceil(earliest.getMinutes() / 15) * 15
  if (roundUp >= 60) {
    earliest.setHours(earliest.getHours() + 1, 0, 0, 0)
  } else {
    earliest.setMinutes(roundUp, 0, 0)
  }

  let h   = earliest.getHours()
  let min = earliest.getMinutes()
  let dateStr = toDateStr(earliest)

  // 早於 11:00 → 從 11:00 開始
  if (h < 11) { h = 11; min = 0 }

  // 晚於 20:00 → 改為明日 11:00
  if (h > 20 || (h === 20 && min > 0)) {
    const next = new Date(earliest)
    next.setDate(next.getDate() + 1)
    dateStr = toDateStr(next)
    h = 11; min = 0
  }

  return { date: dateStr, hour: h, minute: min }
}

const { date: defaultDate, hour: defaultHour, minute: defaultMinute } = getDefaultDateTime()

// 表單狀態
const form = ref({
  date:     defaultDate,
  hour:     defaultHour,
  minute:   defaultMinute,
  adults:   2,
  children: 0,
  name:     '',
  phone:    '',
  email:    '',
  remark:   ''
})

const errors       = ref({})
const availability = ref(null)
const submitting   = ref(false)
const successModal = ref(false)
const bookingNumber = ref('')

let debounceTimer = null

// 日期時間變更時觸發即時查詢
function onDateTimeChange() {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    if (form.value.date && form.value.hour !== '' && form.value.minute !== '') {
      fetchAvailability()
    }
  }, 500)
}

async function fetchAvailability() {
  const dt = buildDateTime()
  if (!dt) return
  try {
    const res = await apiFetch(
      `/Reservations/Availability?date=${encodeURIComponent(toLocalISOString(dt))}&adults=${form.value.adults}&children=${form.value.children}`
    )
    if (res.ok) availability.value = await res.json()
  } catch { /* 忽略網路錯誤 */ }
}

function buildDateTime() {
  if (!form.value.date || form.value.hour === '' || form.value.minute === '') return null
  const [y, m, day] = String(form.value.date).split('-').map(Number)
  return new Date(y, m - 1, day, Number(form.value.hour), Number(form.value.minute), 0, 0)
}

function toLocalISOString(d) {
  const pad = n => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}:00`
}

// 前端七道驗證
function validate() {
  const e  = {}
  const dt = buildDateTime()
  const now = new Date()

  if (!form.value.date) { e.date = '請選擇訂位日期' }

  if (form.value.hour === '' || form.value.minute === '') {
    e.time = '請選擇訂位時段'
  } else if (dt) {
    const hour   = Number(form.value.hour)
    const minute = Number(form.value.minute)
    if (hour < 11 || hour > 20) e.time = '訂位時段須在 11:00 ~ 20:00 之間'
    else if (hour === 20 && minute > 0) e.time = '最晚訂位時段為 20:00'
    else if (![0, 15, 30, 45].includes(minute)) e.time = '訂位分鐘只允許 00、15、30、45'
    else if (dt < new Date(now.getTime() + 30 * 60000)) e.time = '訂位時間必須在 30 分鐘後'
  }

  const total = form.value.adults + form.value.children
  if (total < 1 || total > 10) e.people = '訂位人數須在 1 ~ 10 人之間'

  if (!form.value.name || form.value.name.length < 2 || form.value.name.length > 50)
    e.name = '請輸入姓名（2 ~ 50 字）'

  if (!/^09\d{8}$/.test(form.value.phone))
    e.phone = '請輸入有效的手機號碼（格式：09xxxxxxxx）'

  if (form.value.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.value.email))
    e.email = 'Email 格式不正確'

  errors.value = e
  return Object.keys(e).length === 0
}

async function submitBooking() {
  if (!validate()) return

  submitting.value = true
  try {
    const dt = buildDateTime()
    const payload = {
      name:            form.value.name,
      phone:           form.value.phone,
      email:           form.value.email || null,
      reservationDate: toLocalISOString(dt),
      adultsCount:     form.value.adults,
      childrenCount:   form.value.children,
      remark:          form.value.remark || null
    }

    const res = await apiFetch('/Reservations', {
      method: 'POST',
      body:   JSON.stringify(payload)
    })

    if (res.ok) {
      const data = await res.json()
      bookingNumber.value = data.bookingNumber
      successModal.value  = true
      resetForm()
    } else {
      let message = '訂位失敗，請稍後再試'
      try {
        const err = await res.json()
        message = err.message || message
      } catch { /* 回應非 JSON 時使用預設訊息 */ }
      show(message, 'error')
    }
  } catch {
    show('網路連線錯誤，請確認後端服務是否啟動', 'error')
  } finally {
    submitting.value = false
  }
}

function resetForm() {
  const { date, hour, minute } = getDefaultDateTime()
  form.value = { date, hour, minute, adults: 2, children: 0, name: '', phone: '', email: '', remark: '' }
  errors.value = {}
  availability.value = null
}

// 登入狀態：自動填入會員資料；並以預設時間觸發桌況查詢
onMounted(async () => {
  // 預設時間已帶入，直接觸發即時桌況
  fetchAvailability()

  if (!authStore.isLoggedIn) return
  try {
    const res = await apiFetch('/members/me')
    if (!res.ok) return
    const data = await res.json()
    if (data.name)  form.value.name  = data.name
    if (data.phone) form.value.phone = data.phone
    if (data.email) form.value.email = data.email
  } catch { /* 靜默，不影響訂位流程 */ }
})
</script>

<style scoped>
/* ── 頁面 ── */
.booking-page {
  min-height: 100vh;
  background: var(--eat-bg);
}

/* ── Hero ── */
.booking-hero {
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
  opacity: 0.75;
}
.hero-line {
  flex: 0 0 3rem;
  height: 1px;
  background: var(--eat-secondary);
  opacity: 0.3;
}
.query-link {
  display: inline-flex;
  align-items: center;
  font-family: var(--font-label);
  font-size: .85rem;
  letter-spacing: .06em;
  color: rgba(226,210,185,.6);
  text-decoration: none;
  border: 1px solid rgba(180,120,30,.3);
  border-radius: var(--eat-radius-sm);
  padding: .5rem 1.1rem;
  transition: color .2s, border-color .2s, background .2s;
  white-space: nowrap;
}
.query-link:hover {
  color: var(--eat-primary);
  border-color: rgba(227,199,107,.5);
  background: rgba(227,199,107,.06);
}

/* ── 表單容器 ── */
.form-wrapper {
  max-width: 760px;
  margin: 0 auto;
  background: var(--eat-surface-container);
  border: 1px solid rgba(180,120,30,.18);
  border-radius: 1.25rem;
  padding: 2.5rem 2rem;
}
@media (max-width: 576px) {
  .form-wrapper { padding: 1.5rem 1rem; }
}

/* ── 表單元素 ── */
.form-label-eat {
  display: block;
  font-size: .875rem;
  color: rgba(226,210,185,.75);
  margin-bottom: .4rem;
  letter-spacing: .04em;
}
.required-star { color: #e05a5a; }
.label-hint {
  font-size: .78rem;
  color: rgba(226,210,185,.4);
  font-weight: 400;
}

.form-control-eat,
.form-select-eat {
  width: 100%;
  background: rgba(255,255,255,.05);
  border: 1px solid rgba(180,120,30,.25);
  border-radius: .5rem;
  color: var(--eat-on-surface);
  padding: .65rem .9rem;
  font-size: .95rem;
  font-family: var(--font-body);
  transition: border-color .2s, box-shadow .2s;
  appearance: auto;
}
.form-control-eat::placeholder { color: rgba(226,210,185,.3); }
.form-control-eat:focus,
.form-select-eat:focus {
  outline: none;
  border-color: rgba(227,199,107,.55);
  box-shadow: 0 0 0 3px rgba(227,199,107,.1);
  background: rgba(255,255,255,.07);
}
.form-control-eat.is-invalid,
.form-select-eat.is-invalid { border-color: #e05a5a; }
.form-select-eat option { background: #1e120b; color: var(--eat-on-surface); }
textarea.form-control-eat { resize: vertical; min-height: 88px; }

/* 原生日期選擇器深色主題 */
input[type="date"]::-webkit-calendar-picker-indicator {
  filter: invert(0.7);
  cursor: pointer;
}

.field-error {
  color: #e05a5a;
  font-size: .8rem;
  margin-top: .3rem;
}

/* ── 人數加減器 ── */
.number-input {
  display: inline-flex;
  align-items: center;
  gap: .75rem;
  background: rgba(255,255,255,.05);
  border: 1px solid rgba(180,120,30,.25);
  border-radius: .5rem;
  padding: .5rem 1rem;
}
.num-btn {
  background: none;
  border: none;
  color: var(--eat-primary);
  font-size: 1.1rem;
  line-height: 1;
  cursor: pointer;
  padding: 0 .25rem;
  transition: opacity .15s;
}
.num-btn:hover { opacity: .75; }
.num-value {
  font-size: 1.15rem;
  font-weight: 600;
  color: var(--eat-on-surface);
  min-width: 1.5rem;
  text-align: center;
}

/* ── 即時桌況回饋 ── */
.availability-feedback {
  display: flex;
  align-items: center;
  padding: .85rem 1.1rem;
  border-radius: .5rem;
  font-size: .9rem;
  font-weight: 500;
}
.availability-feedback.available {
  background: rgba(60,179,113,.12);
  color: #3cb371;
  border: 1px solid rgba(60,179,113,.35);
}
.availability-feedback.unavailable {
  background: rgba(192,57,43,.12);
  color: #e05a50;
  border: 1px solid rgba(192,57,43,.35);
}

/* ── 分隔線 ── */
.form-divider {
  border: none;
  border-top: 1px solid rgba(180,120,30,.15);
}

/* ── 送出按鈕 ── */
.submit-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: .85rem 3rem;
  background: var(--eat-primary);
  color: var(--eat-on-primary);
  border: none;
  border-radius: var(--eat-radius-sm);
  font-family: var(--font-label);
  font-size: .95rem;
  font-weight: 600;
  letter-spacing: .08em;
  cursor: pointer;
  transition: background .2s, box-shadow .2s, opacity .2s;
  box-shadow: 0 4px 20px rgba(227,199,107,.25);
}
.submit-btn:hover:not(:disabled) {
  background: #f0d060;
  box-shadow: 0 6px 28px rgba(227,199,107,.4);
}
.submit-btn:disabled {
  opacity: .45;
  cursor: not-allowed;
  box-shadow: none;
}

/* ── 成功 Modal ── */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,.75);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
  padding: 1rem;
}
.success-modal {
  position: relative;
  background: linear-gradient(145deg, #1e100b 0%, #2b1c16 60%, #1a0a05 100%);
  border: 1px solid rgba(227,199,107,.3);
  border-radius: 1.25rem;
  padding: 3rem 2.5rem;
  text-align: center;
  max-width: 440px;
  width: 100%;
  box-shadow: 0 24px 64px rgba(0,0,0,.6), inset 0 1px 0 rgba(255,255,255,.05);
  overflow: hidden;
}
.success-glow {
  position: absolute;
  top: -80px; left: 50%;
  transform: translateX(-50%);
  width: 300px; height: 200px;
  background: radial-gradient(ellipse, rgba(227,199,107,.15) 0%, transparent 70%);
  pointer-events: none;
}
.success-icon-wrap {
  width: 64px; height: 64px;
  border-radius: 50%;
  background: rgba(227,199,107,.12);
  border: 1px solid rgba(227,199,107,.3);
  display: flex; align-items: center; justify-content: center;
  margin: 0 auto 1.25rem;
  font-size: 1.75rem;
  color: var(--eat-primary);
}
.success-title {
  font-family: var(--font-display);
  font-size: 1.6rem;
  color: var(--eat-primary);
  margin-bottom: .4rem;
}
.success-sub {
  font-size: .9rem;
  color: rgba(226,210,185,.55);
  margin-bottom: 1.5rem;
}
.booking-number-card {
  background: rgba(255,255,255,.05);
  border: 1px solid rgba(180,120,30,.2);
  border-radius: .75rem;
  padding: 1.1rem 1.5rem;
  margin-bottom: 1rem;
}
.bn-label {
  font-size: .72rem;
  letter-spacing: .1em;
  color: rgba(226,210,185,.4);
  text-transform: uppercase;
  margin-bottom: .3rem;
}
.bn-value {
  font-family: var(--font-display);
  font-size: 1.8rem;
  font-weight: 700;
  letter-spacing: .18em;
  color: var(--eat-primary);
  text-shadow: 0 0 20px rgba(227,199,107,.3);
}
.success-hint {
  font-size: .8rem;
  color: rgba(226,210,185,.35);
}

/* Modal 按鈕 */
.btn-primary-eat {
  display: inline-flex; align-items: center;
  padding: .6rem 1.6rem;
  background: var(--eat-primary); color: var(--eat-on-primary);
  border: none; border-radius: var(--eat-radius-sm);
  font-family: var(--font-label); font-size: .85rem; font-weight: 600;
  letter-spacing: .08em; cursor: pointer;
  transition: background .2s, box-shadow .2s;
  box-shadow: 0 4px 16px rgba(227,199,107,.25);
}
.btn-primary-eat:hover { background: #f0d060; }

.btn-ghost-eat {
  display: inline-flex; align-items: center; text-decoration: none;
  padding: .6rem 1.6rem;
  background: transparent; color: rgba(226,210,185,.7);
  border: 1px solid rgba(180,120,30,.3); border-radius: var(--eat-radius-sm);
  font-family: var(--font-label); font-size: .85rem; letter-spacing: .08em;
  transition: color .2s, border-color .2s, background .2s;
}
.btn-ghost-eat:hover { color: var(--eat-primary); border-color: rgba(227,199,107,.45); background: rgba(227,199,107,.05); }

/* ── Modal 動畫 ── */
.modal-fade-enter-active,
.modal-fade-leave-active { transition: opacity .25s ease; }
.modal-fade-enter-active .success-modal,
.modal-fade-leave-active .success-modal { transition: transform .25s cubic-bezier(.22,1,.36,1), opacity .25s ease; }
.modal-fade-enter-from { opacity: 0; }
.modal-fade-enter-from .success-modal { transform: translateY(24px) scale(.97); opacity: 0; }
.modal-fade-leave-to { opacity: 0; }
</style>
