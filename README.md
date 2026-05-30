# 義起吃（EatTogether）前台顧客端

> 義起吃前台為義式餐廳顧客端，以 **Vue 3（Composition API）** 開發，透過 Web API 與後台共用同一 SQL Server 資料庫。

---

## 目錄
- [專案簡介](#專案簡介)
- [功能總覽](#功能總覽)
- [技術架構](#技術架構)
- [專案結構](#專案結構)
- [API 對接一覽](#api-對接一覽)
- [開發注意事項](#開發注意事項)

---

## 專案簡介

義起吃前台提供顧客菜單瀏覽、套餐查看、限定餐點訂閱、收藏管理等完整互動體驗。前台透過 **Polling 機制**即時同步後台異動，確保資料一致性。

---

## 功能總覽

### 首頁（Home.vue）
- Hero Banner：查看菜單快速入口
- **招牌料理輪播（Swiper）**：
  - 第一張為義式風味宣傳影片（影片載入失敗時自動 fallback 靜態圖）
  - 後續 Slides 動態呼叫 `GET /api/Dishes/active`，每個分類各取前 3 筆，依餐點屬性顯示推薦 / 人氣 / 限定徽章
  - 支援 Ken Burns 縮放動畫、Fade 切換效果、自動輪播

### 精選菜單（Menu.vue）
- 透過 Polling 定期呼叫 `GET /api/Dishes/active`，自動偵測後台異動並即時更新畫面
- 多維度篩選：人氣餐點、主廚推薦、有辣、素食、我的最愛
- 關鍵字搜尋（餐點名稱 / 描述）
- 卡片 / 列表雙模式切換，排序功能（預設 / 價格 / 評分）
- 分類頁籤導覽
- 庫存連動：後台餐點已售完，前台以 3 秒顯示遮罩無法點擊
- 餐點詳細 Modal：
  - 食材清單（點擊食材可即時查詢 AI 資訊）
  - 餐點屬性（辣度、素食標示）
  - 評分、留言
  - 社群分享（LINE / Facebook / X / 複製連結）

### 精選套餐（SetMeal.vue）
- 呼叫 `GET /api/SetMeals/active`，依後台設定的時間區間過濾當下有效套餐
- 套餐卡片列表，支援比較功能（最多 2 組同時比較）
- 套餐詳細 Modal：
  - 品項組合（必選 / 選配 / 分組顯示）
  - 時段標示、Spotlight 互動效果
  - 拖曳卡片滑鼠特效（3D 視差）

### 本季限定（Limited.vue）
- 呼叫 `GET /api/Dishes/active` 後過濾 `isLimited = true` 餐點
- 狀態區分：即將開始 / 供應中（含截止倒數計時）/ 已售完
- 卡片顯示光罩以表稀缺感
- **限定截止提醒訂閱**：登入後可訂閱 / 取消特定餐點的截止日期前提醒，系統將於截止前自動寄送 Email 通知
  - 訂閱：`POST /api/LimitedNotifications/{dishId}`
  - 取消訂閱：`DELETE /api/LimitedNotifications/{dishId}`
  - 觸發寄信（開發用）：`POST /api/LimitedNotifications/SendReminders`
  - 收藏功能整合（同 Menu.vue）

### 收藏管理（FavoriteButton.vue）
- 登入狀態下呼叫 `GET /api/Favorites` 載入已收藏清單
- 點擊愛心按鈕即時加入 / 取消收藏
  - 加入：`POST /api/Favorites/{dishId}`
  - 取消：`DELETE /api/Favorites/{dishId}`
- 未登入時收藏狀態暫存於 `localStorage`，登入後透過 `POST /api/Favorites/Sync` 同步至伺服器

### 餐點評分（DishRatingSection.vue）
- 登入後可對餐點進行 1–5 星評分（`POST /api/Dishes/{id}/Rate`）
- 以 `localStorage` 記錄已評分紀錄，防止同一帳號重複評分
- Hover 互動星星動態預覽，評分後即時回傳最新平均分與人數

### 留言區（ReviewSection.vue）
- 呼叫 `GET /api/Reviews/{dishId}` 載入該餐點留言列表
- 預設顯示前 5 則，可展開查看全部
- 登入後可送出留言（`POST /api/Reviews/{dishId}`），最多 200 字，即時顯示字數計算
- 相對時間顯示（例：「3 分鐘前」）

### 食材資訊查詢（IngredientCard.vue）
- 點擊食材標籤後呼叫 `POST /api/Ingredients/info`，向後端 Gemini AI 查詢食材資訊
- 回傳四大區塊：來源產地 / 烹煮方式 / 過敏原提示 / 營養價值
- 前端 `Map` 快取：同一食材在同一頁面生命週期內僅查詢一次 API，節省後端費用

### 社群分享（ShareMenu.vue）
- 可選 LINE / Facebook / X / 複製連結四種分享方式
- 分享連結帶入餐點 `id` Query String，供好友直接開啟對應餐點 Modal
- 點擊外部自動收合下拉選單

---

## 技術架構

| 層次 | 技術 |
|---|---|
| 前端框架 | Vue 3（Composition API + `<script setup>`）|
| 狀態管理 | Pinia（`useAuthStore`）|
| 路由 | Vue Router（Query String 深連結）|
| HTTP 請求 | 自封裝 `apiFetch`（含 JWT 自動附加）|
| 即時同步 | Polling（`setInterval` + fingerprint 比對，避免不必要重繪）|
| 本地端快取 | `localStorage`（收藏、評分紀錄）|
| UI 框架 | Bootstrap 5 |
| 輪播元件 | Swiper（Fade 切換 + 自動輪播）|
| AI 食材查詢 | Gemini API（由後端 `IngredientsController` 代理）|
| 版本控制 | Git / GitHub |

---

## 專案結構

```
src/
├── views/
│   ├── Home.vue                  # Swiper（Fade 切換 + 自動輪播）
│   ├── Menu.vue                  # 精選菜單主頁（含 Modal、Polling、評分、留言、收藏、分享）
│   ├── SetMeal.vue               # 精選套餐（品項 Modal、套餐比較）
│   └── Limited.vue               # 本季限定（截止提醒訂閱、倒數計時、收藏）
│
├── components/
│   ├── FavoriteButton.vue        # 愛心收藏按鈕（可複用）
│   ├── DishRatingSection.vue     # 星星評分區塊（含 localStorage 防重複）
│   ├── ReviewSection.vue         # 留言列表 + 送出表單
│   ├── IngredientCard.vue        # 食材資訊卡片（Gemini AI 查詢 + 前端快取）
│   └── ShareMenu.vue             # 分享按鈕下拉（LINE / FB / X / 複製）
│
├── stores/
│   └── auth.js                   # Pinia 驗證狀態（isLoggedIn、member）
│
└── utils/
    └── apiFetch.js               # 封裝 fetch（自動附加 JWT Token）
```

---

## API 對接一覽

| 功能 | 方法 | 端點 |
|---|---|---|
| 首頁招牌料理輪播 | GET | `/api/Dishes/active` |
| 取得啟用餐點 | GET | `/api/Dishes/active` |
| 取得啟用套餐 | GET | `/api/SetMeals/active` |
| 餐點評分 | POST | `/api/Dishes/{id}/Rate` |
| 取得評分 | GET | `/api/Dishes/{id}/Rating` |
| 取得留言 | GET | `/api/Reviews/{dishId}` |
| 送出留言 | POST | `/api/Reviews/{dishId}` |
| 取得收藏清單 | GET | `/api/Favorites` |
| 加入收藏 | POST | `/api/Favorites/{dishId}` |
| 取消收藏 | DELETE | `/api/Favorites/{dishId}` |
| 同步本地收藏 | POST | `/api/Favorites/Sync` |
| 取得限定訂閱清單 | GET | `/api/LimitedNotifications` |
| 訂閱截止提醒 | POST | `/api/LimitedNotifications/{dishId}` |
| 取消截止提醒 | DELETE | `/api/LimitedNotifications/{dishId}` |
| 觸發提醒寄信 | POST | `/api/LimitedNotifications/SendReminders` |
| 食材 AI 查詢 | POST | `/api/Ingredients/info` |

---

## 開發注意事項

- **JWT 驗證**：`apiFetch` 會自動從 `useAuthStore` 取得 Token 附加於 `Authorization` Header，收藏、評分、留言、限定提醒等會員功能皆需登入。
- **Polling 機制**：Menu.vue 透過 fingerprint（餐點 id + 名稱雜湊）比對，僅在後台有異動時才觸發畫面更新，降低不必要的重繪效能損耗。
- **收藏同步策略**：未登入時以 `localStorage` 暫存，登入後呼叫 Sync API 以 merge（不覆蓋）方式寫入伺服器，避免跨裝置遺失。
- **食材快取**：`IngredientCard.vue` 以頁面層級 `Map` 快取查詢結果，同一食材在元件生命週期內僅打一次 Gemini API，節省後端費用。
- **深連結**：Menu.vue 與 Limited.vue 支援 Query String `?dish={id}` 直接開啟對應餐點 Modal，方便社群分享後跳轉。
- **限定截止提醒**：系統於限定餐點截止日期前自動觸發寄信，訂閱者將收到 Email 通知，提醒把握最後供應時間。

## 畫面截圖

### 首頁(菜品輪播)
<img width="1837" height="907" alt="大專-首頁" src="https://github.com/user-attachments/assets/7cb94dc6-b77d-48b7-8366-c79043ba83b2" />

### 菜單與餐點詳情
<img width="1462" height="875" alt="菜單" src="https://github.com/user-attachments/assets/8d9859cb-d8c3-420d-b2d9-90f201a523bb" />

### AI 食材即時查詢
<img width="592" height="422" alt="菜單AI即時查詢" src="https://github.com/user-attachments/assets/635f8dbd-d063-4c93-962e-d1aedd2ca8f9" />

### 登入後評分留言
<img width="652" height="435" alt="登入後可留言評分" src="https://github.com/user-attachments/assets/729b6918-0c34-4fb0-885f-5cc7023c96da" />

### 限定餐點截止提醒
<img width="635" height="587" alt="限定商品小鈴鐺通知" src="https://github.com/user-attachments/assets/afb05c55-942b-4601-844f-3e9680a18c35" />

### 套餐比較
<img width="1010" height="887" alt="套餐詳細比較" src="https://github.com/user-attachments/assets/c1176355-825b-4793-8f1b-3a78f0a26e15" />
