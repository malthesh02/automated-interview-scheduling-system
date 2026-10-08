/* ============================================================
   INTERVIEWPRO — APP.JS  (Fixed)
   ============================================================ */

   const API_BASE = '/api';          // FIX 1: relative URL (was absolute http://localhost:5000/api - breaks CORS)
   let authToken = localStorage.getItem('authToken');
   
   /* ── INIT ── */
   document.addEventListener('DOMContentLoaded', () => {
       checkAuth();          // FIX 2: actually call checkAuth on load (was never called)
       loadUserInfo();
       initNav();
       initHamburger();
       initHeaderButtons();  // FIX 3: wire up top-bar buttons (logout, schedule, conflicts)
       initViewAllLinks();
       initCharts();
       buildMiniCalendar();
       initSettingsTabs();
       loadDashboardData();
   });
   
   /* ── AUTH GUARD ── */
   function checkAuth() {
       if (!authToken) {
           window.location.href = 'login.html'; // FIX 4: redirect to login (was commented out)
       }
   }
   
   /* ── USER INFO ── */
   const ROLE_NAMES = { 0: 'Admin', 1: 'Interviewer', 2: 'Candidate' };
   
   function getRoleName(role) {
       // role may come as int (0/1/2) or string ("Admin"/"Interviewer"/"Candidate")
       if (typeof role === 'number') return ROLE_NAMES[role] || String(role);
       if (typeof role === 'string' && ROLE_NAMES[parseInt(role)] !== undefined && !isNaN(parseInt(role))) {
           return ROLE_NAMES[parseInt(role)];
       }
       return role || 'Unknown';
   }
   
   function loadUserInfo() {
       const raw = localStorage.getItem('user');
       if (!raw) return;
       try {
           const user = JSON.parse(raw);
           const roleName = getRoleName(user.role);
           // Populate settings fields
           const sn = document.getElementById('settingsName');
           const se = document.getElementById('settingsEmail');
           const sr = document.getElementById('settingsRole');
           if (sn) sn.value = user.name || '';
           if (se) se.value = user.email || '';
           if (sr) sr.value = roleName;   // FIX: show "Candidate" not "2"
           // FIX 5: use correct IDs from the HTML (sbAvatar, sbName, sbRole)
           const nm = document.getElementById('sbName');
           const rl = document.getElementById('sbRole');
           const av = document.getElementById('sbAvatar');
           if (nm) nm.textContent = user.name || 'Admin User';
           if (rl) rl.textContent = roleName;
           if (av) av.src = `https://ui-avatars.com/api/?name=${encodeURIComponent(user.name||'Admin')}&background=667eea&color=fff`;
           const greet = document.getElementById('dashGreet');
           if (greet) greet.textContent = `Welcome back, ${user.name||'Admin'}! Here's what's happening today.`;
   
           // Hide nav items the logged-in role can't access
           const roleInt = typeof user.role === 'number' ? user.role : parseInt(user.role);
           applyRoleVisibility(roleInt);
       } catch (e) {}
   }
   
   function applyRoleVisibility(roleInt) {
       // roleInt: 0=Admin, 1=Interviewer, 2=Candidate
       if (roleInt === 2) {
           // Candidates cannot access the Candidates list (API returns 403) or Conflicts
           document.querySelectorAll('.nav-item[data-page="conflicts"]').forEach(el => el.style.display = 'none');
           document.querySelectorAll('.nav-item[data-page="candidates"]').forEach(el => el.style.display = 'none');
       }
       if (roleInt === 1) {  // Interviewer
           const btn = document.getElementById('topScheduleBtn');
           if (btn) btn.style.display = 'none';
       }
   }
   
   /* ── NAVIGATION ── */
   function initNav() {
       document.querySelectorAll('.nav-item').forEach(item => {
           item.addEventListener('click', e => {
               e.preventDefault();
               navigateTo(item.dataset.page);
           });
       });
   }
   
   // FIX 6: define navigateTo (called from HTML but was never defined)
   function navigateTo(name) {
       // switch .page-content elements
       document.querySelectorAll('.page-content').forEach(p => p.classList.remove('active'));
       const target = document.getElementById(name);
       if (target) {
           target.classList.add('active');
           if (name === 'analytics')    initAnalyticsCharts();
           if (name === 'interviews')   loadInterviews();
           if (name === 'candidates')   loadCandidates();
           if (name === 'interviewers') loadInterviewers();
           if (name === 'conflicts')    loadConflicts();
           if (name === 'availability') loadAvailability();
       }
       // Load page-specific data
       if (name === 'analytics') loadAnalyticsData();
       if (name === 'candidates') loadCandidates();
       if (name === 'interviewers') loadInterviewers();
       if (name === 'conflicts') loadConflicts();
       if (name === 'interviews') loadInterviews();
       if (name === 'availability') loadAvailability();
   
       document.querySelectorAll('.nav-item').forEach(n => {
           n.classList.toggle('active', n.dataset.page === name);
       });
       const sidebar = document.getElementById('sidebar');
       if (sidebar) sidebar.classList.remove('open');
   }
   
   function switchPage(name) { navigateTo(name); }
   
   function initViewAllLinks() {
       document.querySelectorAll('[data-page]').forEach(a => {
           if (a.classList.contains('nav-item')) return;
           a.addEventListener('click', e => {
               e.preventDefault();
               navigateTo(a.dataset.page);
           });
       });
   }
   
   /* ── HAMBURGER ── */
   function initHamburger() {
       // FIX 8: correct ID is 'menuToggle' not 'hamburger'
       const btn     = document.getElementById('menuToggle');
       const sidebar = document.getElementById('sidebar');
       const overlay = document.getElementById('sidebarOverlay');
       if (btn && sidebar) btn.addEventListener('click', () => sidebar.classList.toggle('open'));
       if (overlay)        overlay.addEventListener('click', () => sidebar && sidebar.classList.remove('open'));
   }
   
   /* ── HEADER BUTTONS ── */
   // FIX 9: wire all header buttons that had no event listeners
   function initHeaderButtons() {
       const logoutBtn = document.getElementById('logoutBtn');
       if (logoutBtn) {
           logoutBtn.addEventListener('click', e => {
               e.preventDefault();
               localStorage.removeItem('authToken');
               localStorage.removeItem('user');
               window.location.href = 'login.html';
           });
       }
       const scheduleBtn = document.getElementById('topScheduleBtn');
       if (scheduleBtn) scheduleBtn.addEventListener('click', () => openScheduleModalWithData());
   
       const notifBtn = document.getElementById('notifBtn');
       if (notifBtn) notifBtn.addEventListener('click', () => navigateTo('conflicts'));
   }
   
   /* ── MODALS ── */
   // AFTER
function openModal(id) {
    const el = document.getElementById(id);
    if (el) { el.classList.add('open'); document.body.style.overflow = 'hidden'; }
}

function closeModal(id) {
    const el = document.getElementById(id);
    if (el) { el.classList.remove('open'); document.body.style.overflow = ''; }
}

document.addEventListener('keydown', e => {
    if (e.key === 'Escape') {
        document.querySelectorAll('.modal-overlay.open').forEach(m => {
            m.classList.remove('open');
            document.body.style.overflow = '';
        });
    }
});
   
   /* ── TOAST ── */
   function showToast(msg, type = 'success') {
       const t = document.getElementById('toast');
       if (!t) return;
       const icons = { success: 'fas fa-check-circle', error: 'fas fa-exclamation-circle', info: 'fas fa-info-circle', warning: 'fas fa-exclamation-triangle' };
       const ico = document.getElementById('toastIcon');
       const txt = document.getElementById('toastMsg');
       if (ico) ico.className = icons[type] || icons.success;
       if (txt) txt.textContent = msg;
       else t.textContent = msg;
       t.className = `toast-${type} show`;
       setTimeout(() => t.className = '', 3500);
   }
   
   /* ── API HELPERS ── */
   async function apiGet(endpoint) {
       const res = await fetch(`${API_BASE}${endpoint}`, {
           headers: { 'Authorization': `Bearer ${authToken}`, 'Content-Type': 'application/json' }
       });
       if (res.status === 401) { localStorage.removeItem('authToken'); window.location.href = 'login.html'; return null; }
       if (!res.ok) throw new Error(`API Error ${res.status}`);
       return res.json();
   }
   
   async function apiPost(endpoint, body) {
       const res = await fetch(`${API_BASE}${endpoint}`, {
           method: 'POST',
           headers: { 'Authorization': `Bearer ${authToken}`, 'Content-Type': 'application/json' },
           body: JSON.stringify(body)
       });
       if (res.status === 401) { localStorage.removeItem('authToken'); window.location.href = 'login.html'; return null; }
       if (!res.ok) {
           const err = await res.json().catch(() => ({}));
           throw new Error(err.message || `API Error ${res.status}`);
       }
       return res.json();
   }
   
   async function apiPut(endpoint, body) {
       const res = await fetch(`${API_BASE}${endpoint}`, {
           method: 'PUT',
           headers: { 'Authorization': `Bearer ${authToken}`, 'Content-Type': 'application/json' },
           body: JSON.stringify(body)
       });
       if (res.status === 401) { localStorage.removeItem('authToken'); window.location.href = 'login.html'; return null; }
       if (!res.ok) {
           const err = await res.json().catch(() => ({}));
           throw new Error(err.message || `API Error ${res.status}`);
       }
       return res.json();
   }
   
   async function apiDelete(endpoint) {
       const res = await fetch(`${API_BASE}${endpoint}`, {
           method: 'DELETE',
           headers: { 'Authorization': `Bearer ${authToken}` }
       });
       if (res.status === 401) { localStorage.removeItem('authToken'); window.location.href = 'login.html'; return null; }
       if (!res.ok) throw new Error(`API Error ${res.status}`);
       try { return res.json(); } catch { return {}; }
   }
   
   /* ─────────────────────────────────────────
      DASHBOARD
   ───────────────────────────────────────── */
   async function loadDashboardData() {
       try {
           // FIX: use date-range endpoint for interviews; /Candidates works as-is; use /Conflicts/unresolved
           const startDate = new Date(); startDate.setMonth(startDate.getMonth() - 1);
           const endDate   = new Date(); endDate.setMonth(endDate.getMonth() + 3);
           const start = startDate.toISOString().slice(0, 10);
           const end   = endDate.toISOString().slice(0, 10);
           const [ivRes, candRes, cflRes] = await Promise.allSettled([
               apiGet(`/Interviews/date-range?startDate=${start}&endDate=${end}`),
               apiGet('/Candidates'),
               apiGet('/Conflicts/unresolved')
           ]);
           const ivs   = ivRes.status  === 'fulfilled' && ivRes.value   ? ivRes.value   : [];
           const cands = candRes.status === 'fulfilled' && candRes.value ? candRes.value : [];
           const cfls  = cflRes.status  === 'fulfilled' && cflRes.value  ? cflRes.value  : [];
   
           const today = new Date().toISOString().slice(0, 10);
           // FIX: interviewDate (not scheduledDate)
           const todayCount  = ivs.filter(i => i.interviewDate && i.interviewDate.startsWith(today)).length;
           const openConflicts = cfls.filter(c => !c.isResolved).length;
   
           setText('statTotal',     ivs.length);
           setText('statCandidates', cands.length);
           setText('statToday',     todayCount);
           setText('statConflicts', openConflicts);
           setConflictBadge(openConflicts);
           renderRecentInterviews(ivs.slice(0, 5), cands);
       } catch (e) {
           console.warn('Dashboard load error:', e.message);
       }
   }
   
   function setText(id, val) { const el = document.getElementById(id); if (el) el.textContent = val; }
   
   function setConflictBadge(count) {
       ['conflictBadge','conflictNavBadge'].forEach(id => {
           const el = document.getElementById(id);
           if (!el) return;
           el.textContent = count;
           el.style.display = count > 0 ? '' : 'none';
       });
   }
   
   function renderRecentInterviews(interviews, candidates) {
       const tbody = document.getElementById('dashRecentBody');
       if (!tbody) return;
       if (!interviews.length) {
           tbody.innerHTML = '<tr><td colspan="5" class="loading-row">No interviews yet.</td></tr>';
           return;
       }
       const statusMap = { Scheduled:'status-scheduled', Completed:'status-completed', Cancelled:'status-cancelled', Rescheduled:'status-rescheduled' };
       tbody.innerHTML = interviews.map(iv => {
           const cand = candidates.find(c => c.candidateId === iv.candidateId);
           const candName = cand ? cand.name : (iv.candidateName || `Candidate #${iv.candidateId}`);
           // FIX: interviewDate (not scheduledDate); status is enum string
           const date = iv.interviewDate ? new Date(iv.interviewDate).toLocaleString() : '—';
           const statusLabel = iv.status || 'Unknown';
           const statusClass = statusMap[iv.status] || '';
           return `<tr>
               <td>${escHtml(candName)}</td>
               <td>${escHtml(iv.interviewerName || '—')}</td>
               <td>${date}</td>
               <td><span class="status-badge ${statusClass}">${statusLabel}</span></td>
               <td>
                   <button class="btn-icon" title="View" onclick="viewInterview(${iv.interviewId})"><i class="fas fa-eye"></i></button>
                   <button class="btn-icon danger" title="Cancel" onclick="cancelInterview(${iv.interviewId})"><i class="fas fa-times"></i></button>
               </td>
           </tr>`;
       }).join('');
   }
   
   /* ─────────────────────────────────────────
      INTERVIEWS PAGE
   ───────────────────────────────────────── */
   async function loadInterviews() {
       const tbody = document.getElementById('interviewsBody');
       if (!tbody) return;
       tbody.innerHTML = '<tr><td colspan="6" class="loading-row"><i class="fas fa-spinner fa-spin"></i> Loading...</td></tr>';
       try {
           // FIX 1: Use correct date-range endpoint (no bare GET /api/Interviews exists)
           const startDate = new Date();
           startDate.setMonth(startDate.getMonth() - 1);
           const endDate = new Date();
           endDate.setMonth(endDate.getMonth() + 3);
           const start = startDate.toISOString().slice(0, 10);
           const end   = endDate.toISOString().slice(0, 10);
           const data = await apiGet(`/Interviews/date-range?startDate=${start}&endDate=${end}`);
           if (!data || !data.length) { tbody.innerHTML = '<tr><td colspan="6" class="loading-row">No interviews found.</td></tr>'; return; }
           const statusMap   = { Scheduled:'status-scheduled', Completed:'status-completed', Cancelled:'status-cancelled', Rescheduled:'status-rescheduled' };
           tbody.innerHTML = data.map(iv => {
               // FIX 1b: field is interviewDate (not scheduledDate); status is enum string not int index
               const date = iv.interviewDate ? new Date(iv.interviewDate).toLocaleString() : '—';
               const statusLabel = iv.status || 'Unknown';
               const statusClass = statusMap[iv.status] || '';
               return `<tr>
                   <td>#${iv.interviewId}</td>
                   <td>${escHtml(iv.candidateName || '—')}</td>
                   <td>${escHtml(iv.interviewerName || '—')}</td>
                   <td>${date}</td>
                   <td><span class="status-badge ${statusClass}">${statusLabel}</span></td>
                   <td>
                       <button class="btn-icon" title="View" onclick="viewInterview(${iv.interviewId})"><i class="fas fa-eye"></i></button>
                       <button class="btn-icon danger" title="Cancel" onclick="cancelInterview(${iv.interviewId})"><i class="fas fa-times"></i></button>
                   </td>
               </tr>`;
           }).join('');
       } catch (e) {
           tbody.innerHTML = `<tr><td colspan="6" class="loading-row" style="color:#DA1E28">Failed to load: ${e.message}</td></tr>`;
       }
   }
   
   async function viewInterview(id) {
       openModal('viewModal');
       const body = document.getElementById('viewModalBody');
       if (body) body.innerHTML = '<p><i class="fas fa-spinner fa-spin"></i> Loading...</p>';
       try {
           const iv = await apiGet(`/Interviews/${id}`);
           if (!iv) return;
           // FIX: interviewDate (not scheduledDate); status is enum string; id is interviewId
           const date = iv.interviewDate ? new Date(iv.interviewDate).toLocaleString() : '—';
           const statusLabel = iv.status || 'Unknown';
           body.innerHTML = `<div style="display:grid;grid-template-columns:1fr 1fr;gap:16px;padding:8px 0">
               <div><strong>ID:</strong> #${iv.interviewId}</div>
               <div><strong>Status:</strong> ${statusLabel}</div>
               <div><strong>Candidate:</strong> ${escHtml(iv.candidateName||'—')}</div>
               <div><strong>Interviewer:</strong> ${escHtml(iv.interviewerName||'—')}</div>
               <div><strong>Date:</strong> ${date}</div>
               <div><strong>Time:</strong> ${iv.startTime||'—'} – ${iv.endTime||'—'}</div>
           </div>`;
       } catch (e) {
           if (body) body.innerHTML = `<p style="color:#DA1E28">Failed to load: ${e.message}</p>`;
       }
   }
   
   async function cancelInterview(id) {
       if (!confirm('Are you sure you want to cancel this interview?')) return;
       try {
           await apiDelete(`/Interviews/${id}`);
           showToast('Interview cancelled successfully.');
           loadInterviews();
           loadDashboardData();
       } catch (e) {
           showToast('Failed to cancel: ' + e.message, 'error');
       }
   }
   
   async function openScheduleModalWithData() {
       openModal('scheduleModal');
       try {
           const [cands, irs] = await Promise.all([apiGet('/Candidates'), apiGet('/Interviewers')]);
           const mCand = document.getElementById('mCand');
           const mIr   = document.getElementById('mIr');
           if (mCand && cands) mCand.innerHTML = '<option value="">Select candidate...</option>' +
               cands.map(c => `<option value="${c.candidateId}">${escHtml(c.name)}</option>`).join('');
           if (mIr && irs)     mIr.innerHTML   = '<option value="">Select interviewer...</option>' +
               irs.map(i => `<option value="${i.interviewerId}">${escHtml(i.name)}</option>`).join('');
       } catch (e) { /* non-fatal */ }
   }
   
   // FIX 10: define submitSchedule (called from HTML but was missing)
   async function submitSchedule() {
       const candId = document.getElementById('mCand')?.value;
       const irId   = document.getElementById('mIr')?.value;
       const date   = document.getElementById('mDate')?.value;
       const start  = document.getElementById('mStart')?.value;
       const end    = document.getElementById('mEnd')?.value;
       const errEl  = document.getElementById('mErr');
   
       if (!candId || !irId || !date || !start || !end) {
           if (errEl) { errEl.textContent = 'All fields are required.'; errEl.style.display = 'block'; }
           return;
       }
       if (errEl) errEl.style.display = 'none';
       try {
           // FIX 7: API expects interviewDate (DateTime), startTime/endTime as TimeSpan "HH:mm:ss"
           // candidateId and interviewerId (not generic id)
           await apiPost('/Interviews/schedule', {
               candidateId:   +candId,
               interviewerId: +irId,
               interviewDate: `${date}T00:00:00`,
               startTime:     `${start}:00`,
               endTime:       `${end}:00`
           });
           closeModal('scheduleModal');
           showToast('Interview scheduled successfully!');
           loadDashboardData();
           loadInterviews();
       } catch (e) {
           if (errEl) { errEl.textContent = e.message; errEl.style.display = 'block'; }
       }
   }
   
   /* ─────────────────────────────────────────
      CANDIDATES PAGE
   ───────────────────────────────────────── */
   async function loadCandidates() {
       const tbody = document.getElementById('candidatesBody');
       if (!tbody) return;
       // Candidates (role 2) cannot access this endpoint — show friendly message
       try {
           const u = JSON.parse(localStorage.getItem('user') || '{}');
           const roleInt = typeof u.role === 'number' ? u.role : parseInt(u.role);
           if (roleInt === 2) {
               tbody.innerHTML = '<tr><td colspan="5" class="loading-row" style="color:#6B7280">You do not have permission to view candidates.</td></tr>';
               return;
           }
       } catch (_) {}
       tbody.innerHTML = '<tr><td colspan="5" class="loading-row"><i class="fas fa-spinner fa-spin"></i> Loading...</td></tr>';
       try {
           const data = await apiGet('/Candidates');
           if (!data || !data.length) { tbody.innerHTML = '<tr><td colspan="5" class="loading-row">No candidates found.</td></tr>'; return; }
           // FIX 2: Status is an enum string ("Applied","Shortlisted",…); id field is candidateId
           const statusClasses = { Applied:'status-scheduled', Shortlisted:'status-rescheduled', Rejected:'status-cancelled', Hired:'status-completed' };
           const statusIntMap  = { Applied:0, Shortlisted:1, Rejected:2, Hired:3 };
           tbody.innerHTML = data.map(c => `<tr>
               <td>${escHtml(c.name)}</td>
               <td>${escHtml(c.email)}</td>
               <td><span class="status-badge ${statusClasses[c.status]||''}">${c.status||'Unknown'}</span></td>
               <td>${c.resumeLink ? `<a href="${escHtml(c.resumeLink)}" target="_blank">View</a>` : '—'}</td>
               <td><button class="btn-icon" title="Update Status" onclick="openStatusModal(${c.candidateId},${statusIntMap[c.status]??0})"><i class="fas fa-edit"></i></button></td>
           </tr>`).join('');
       } catch (e) {
           tbody.innerHTML = `<tr><td colspan="5" class="loading-row" style="color:#DA1E28">Failed to load: ${e.message}</td></tr>`;
       }
   }
   
   // FIXED: use /api/Auth/register with role=2 (Candidate). Resume optional, NO department.
   async function submitAddCandidate() {
       const name     = document.getElementById('newCandName')?.value.trim();
       const email    = document.getElementById('newCandEmail')?.value.trim();
       const password = document.getElementById('newCandPassword')?.value;
       const resume   = document.getElementById('newCandResume')?.value.trim();
       const errEl    = document.getElementById('candErr');
       if (!name || !email || !password) {
           if (errEl) { errEl.textContent = 'Name, email and password are required.'; errEl.style.display = 'block'; }
           return;
       }
       if (errEl) errEl.style.display = 'none';
       try {
           const payload = { name, email, password, role: 2 };          // role 2 = Candidate
           if (resume) payload.resumeLink = resume;                      // optional
           // department is NOT sent — candidates have no department
           const res = await fetch('/api/Auth/register', {
               method: 'POST',
               headers: { 'Authorization': `Bearer ${authToken}`, 'Content-Type': 'application/json' },
               body: JSON.stringify(payload)
           });
           const data = await res.json().catch(() => ({}));
           if (!res.ok) throw new Error(data.message || 'Registration failed');
           closeModal('addCandidateModal');
           showToast('Candidate added successfully!');
           loadCandidates(); loadDashboardData();
           ['newCandName','newCandEmail','newCandPassword','newCandResume'].forEach(id => { const el = document.getElementById(id); if (el) el.value = ''; });
       } catch (e) {
           if (errEl) { errEl.textContent = e.message; errEl.style.display = 'block'; }
       }
   }
   
   function openStatusModal(id, currentStatus) {
       document.getElementById('statusCandId').value = id;
       const sel = document.getElementById('statusVal');
       if (sel) sel.value = currentStatus;
       openModal('statusModal');
   }
   
   // FIX 12: define submitStatusUpdate (called from HTML but was missing)
   async function submitStatusUpdate() {
       const id  = document.getElementById('statusCandId')?.value;
       const val = document.getElementById('statusVal')?.value;
       if (!id) return;
       try {
           await apiPut(`/Candidates/${id}/status`, { status: +val });
           closeModal('statusModal');
           showToast('Status updated!');
           loadCandidates();
       } catch (e) { showToast('Failed to update: ' + e.message, 'error'); }
   }
   
   /* ─────────────────────────────────────────
      INTERVIEWERS PAGE
   ───────────────────────────────────────── */
   async function loadInterviewers() {
       const tbody = document.getElementById('interviewersBody');
       if (!tbody) return;
       tbody.innerHTML = '<tr><td colspan="5" class="loading-row"><i class="fas fa-spinner fa-spin"></i> Loading...</td></tr>';
       try {
           const data = await apiGet('/Interviewers');
           if (!data || !data.length) { tbody.innerHTML = '<tr><td colspan="5" class="loading-row">No interviewers found.</td></tr>'; return; }
           tbody.innerHTML = data.map(ir => `<tr>
               <td>${escHtml(ir.name)}</td>
               <td>${escHtml(ir.email)}</td>
               <td>${escHtml(ir.department||'—')}</td>
               <td>${ir.interviewCount ?? 0}</td>
               <td><button class="btn-icon" title="Add Availability" onclick="openAvailModal(${ir.interviewerId})"><i class="fas fa-clock"></i></button></td>
           </tr>`).join('');
       } catch (e) {
           tbody.innerHTML = `<tr><td colspan="5" class="loading-row" style="color:#DA1E28">Failed to load: ${e.message}</td></tr>`;
       }
   }
   
   // FIXED: use /api/Auth/register with role=1 (Interviewer). Dept required, NO resumeLink.
   async function submitAddInterviewer() {
       const name     = document.getElementById('newIrName')?.value.trim();
       const email    = document.getElementById('newIrEmail')?.value.trim();
       const password = document.getElementById('newIrPassword')?.value;
       const dept     = document.getElementById('newIrDept')?.value.trim();
       const errEl    = document.getElementById('irErr');
       if (!name || !email || !password) {
           if (errEl) { errEl.textContent = 'Name, email and password are required.'; errEl.style.display = 'block'; }
           return;
       }
       if (!dept) {
           if (errEl) { errEl.textContent = 'Department is required for interviewers.'; errEl.style.display = 'block'; }
           return;
       }
       if (errEl) errEl.style.display = 'none';
       try {
           const payload = { name, email, password, role: 1, department: dept }; // role 1 = Interviewer
           // resumeLink is NOT sent — interviewers have no resume field
           const res = await fetch('/api/Auth/register', {
               method: 'POST',
               headers: { 'Authorization': `Bearer ${authToken}`, 'Content-Type': 'application/json' },
               body: JSON.stringify(payload)
           });
           const data = await res.json().catch(() => ({}));
           if (!res.ok) throw new Error(data.message || 'Registration failed');
           closeModal('addInterviewerModal');
           showToast('Interviewer added successfully!');
           loadInterviewers();
           ['newIrName','newIrEmail','newIrPassword','newIrDept'].forEach(id => { const el = document.getElementById(id); if (el) el.value = ''; });
       } catch (e) {
           if (errEl) { errEl.textContent = e.message; errEl.style.display = 'block'; }
       }
   }
   
   /* ─────────────────────────────────────────
      AVAILABILITY PAGE
   ───────────────────────────────────────── */
   async function loadAvailability() {
       const tbody = document.getElementById('availabilityBody');
       if (!tbody) return;
       tbody.innerHTML = '<tr><td colspan="5" class="loading-row"><i class="fas fa-spinner fa-spin"></i> Loading...</td></tr>';
       try {
           // FIX 4: No global /api/Interviewers/availability endpoint exists.
           // Fetch all interviewers, then fetch available-slots per interviewer.
           const interviewers = await apiGet('/Interviewers');
           if (!interviewers || !interviewers.length) {
               tbody.innerHTML = '<tr><td colspan="5" class="loading-row">No interviewers found.</td></tr>';
               return;
           }
           const startDate = new Date().toISOString().slice(0, 10);
           const endDate   = new Date(Date.now() + 90 * 86400000).toISOString().slice(0, 10);
           const slotResults = await Promise.allSettled(
               interviewers.map(ir =>
                   apiGet(`/Interviewers/${ir.interviewerId}/available-slots?startDate=${startDate}&endDate=${endDate}`)
                       .then(slots => (slots || []).map(s => ({ ...s, interviewerName: ir.name })))
               )
           );
           const allSlots = slotResults
               .filter(r => r.status === 'fulfilled')
               .flatMap(r => r.value || []);
           if (!allSlots.length) {
               tbody.innerHTML = '<tr><td colspan="5" class="loading-row">No availability slots found.</td></tr>';
               return;
           }
           tbody.innerHTML = allSlots.map(slot => `<tr>
               <td>${escHtml(slot.interviewerName || '—')}</td>
               <td>${slot.availableDate ? new Date(slot.availableDate).toLocaleDateString() : '—'}</td>
               <td>${slot.startTime || '—'}</td>
               <td>${slot.endTime || '—'}</td>
               <td><span class="status-badge ${slot.isBooked ? 'status-cancelled' : 'status-completed'}">${slot.isBooked ? 'Booked' : 'Free'}</span></td>
           </tr>`).join('');
       } catch (e) {
           tbody.innerHTML = `<tr><td colspan="5" class="loading-row" style="color:#DA1E28">Failed to load: ${e.message}</td></tr>`;
       }
   }
   
   function openAvailModal(irId) {
       document.getElementById('availIrId').value = irId;
       const d = document.getElementById('availDate');
       if (d) d.min = new Date().toISOString().slice(0,10);
       openModal('availModal');
   }
   
   // FIX 14: define submitAvailability (called from HTML but was missing)
   async function submitAvailability() {
       const irId  = document.getElementById('availIrId')?.value;
       const date  = document.getElementById('availDate')?.value;
       const start = document.getElementById('availStart')?.value;
       const end   = document.getElementById('availEnd')?.value;
       const errEl = document.getElementById('availErr');
       if (!irId || !date || !start || !end) {
           if (errEl) { errEl.textContent = 'All fields are required.'; errEl.style.display = 'block'; }
           return;
       }
       if (errEl) errEl.style.display = 'none';
       try {
           // FIX 8: CreateAvailabilityDto expects availableDate (DateTime), startTime/endTime as TimeSpan "HH:mm:ss"
           await apiPost(`/Interviewers/${irId}/availability`, {
               availableDate: `${date}T00:00:00`,
               startTime: `${start}:00`,
               endTime:   `${end}:00`
           });
           closeModal('availModal');
           showToast('Availability slot added!');
           loadAvailability();
       } catch (e) {
           if (errEl) { errEl.textContent = e.message; errEl.style.display = 'block'; }
       }
   }
   
   /* ─────────────────────────────────────────
      CONFLICTS PAGE
   ───────────────────────────────────────── */
   async function loadConflicts() {
       const tbody = document.getElementById('conflictsBody');
       if (!tbody) return;
       tbody.innerHTML = '<tr><td colspan="5" class="loading-row"><i class="fas fa-spinner fa-spin"></i> Loading...</td></tr>';
       try {
           // FIX 5: GET /api/Conflicts requires Admin-only; use /unresolved (Admin+Interviewer)
           const res = await fetch(`${API_BASE}/Conflicts/unresolved`, {
               headers: { 'Authorization': `Bearer ${authToken}`, 'Content-Type': 'application/json' }
           });
           if (res.status === 401) { localStorage.removeItem('authToken'); window.location.href = 'login.html'; return; }
           if (res.status === 403) {
               tbody.innerHTML = '<tr><td colspan="5" class="loading-row"><i class="fas fa-lock"></i> You don\'t have permission to view conflicts.</td></tr>';
               return;
           }
           if (!res.ok) throw new Error(`API Error ${res.status}`);
           const data = await res.json();
           setConflictBadge(data ? data.filter(c => !c.isResolved).length : 0);
           if (!data || !data.length) { tbody.innerHTML = '<tr><td colspan="5" class="loading-row">No conflicts found. 🎉</td></tr>'; return; }
           tbody.innerHTML = data.map(c => `<tr>
               <td>#${c.conflictId}</td>
               <td>${escHtml(c.conflictReason || '—')}</td>
               <td>${c.detectedAt ? new Date(c.detectedAt).toLocaleString() : '—'}</td>
               <td><span class="status-badge ${c.isResolved ? 'status-completed' : 'status-cancelled'}">${c.isResolved ? 'Resolved' : 'Open'}</span></td>
               <td>${!c.isResolved ? `<button class="btn-icon" title="Resolve" onclick="openResolveModal(${c.conflictId})"><i class="fas fa-check"></i></button>` : '—'}</td>
           </tr>`).join('');
       } catch (e) {
           tbody.innerHTML = `<tr><td colspan="5" class="loading-row" style="color:#DA1E28">Failed to load: ${e.message}</td></tr>`;
       }
   }
   
   function openResolveModal(id) {
       document.getElementById('resolveId').value = id;
       const notes = document.getElementById('resolveNotes');
       if (notes) notes.value = '';
       openModal('resolveModal');
   }
   
   // FIX 15: define submitResolve (called from HTML but was missing)
   async function submitResolve() {
       const id    = document.getElementById('resolveId')?.value;
       const notes = document.getElementById('resolveNotes')?.value.trim();
       if (!id) return;
       try {
           // FIX 6: endpoint is POST /Conflicts/resolve (not PUT /Conflicts/{id}/resolve)
           // payload needs { conflictId: id, resolutionNotes: notes }
           await apiPost(`/Conflicts/resolve`, { conflictId: +id, resolutionNotes: notes || '' });
           closeModal('resolveModal');
           showToast('Conflict resolved!');
           loadConflicts(); loadDashboardData();
       } catch (e) { showToast('Failed to resolve: ' + e.message, 'error'); }
   }
   
   /* ─────────────────────────────────────────
      CHARTS
   ───────────────────────────────────────── */
   function initCharts() { initTrendChart(); initPipelineChart(); }
   
   function initTrendChart() {
       const el = document.getElementById('interviewChart') || document.getElementById('trendChart');
       if (!el) return;
       new Chart(el, {
           type: 'line',
           data: {
               labels: ['Mon','Tue','Wed','Thu','Fri','Sat','Sun'],
               datasets: [
                   { label: 'Scheduled', data: [12,19,15,25,22,18,24], borderColor: '#0F62FE', backgroundColor: 'rgba(15,98,254,0.08)', borderWidth: 2, tension: 0.4, fill: true, pointRadius: 4, pointBackgroundColor: '#0F62FE', pointBorderColor: '#fff', pointBorderWidth: 2 },
                   { label: 'Completed', data: [10,15,13,20,18,15,20], borderColor: '#198038', backgroundColor: 'rgba(25,128,56,0.06)', borderWidth: 2, tension: 0.4, fill: true, pointRadius: 4, pointBackgroundColor: '#198038', pointBorderColor: '#fff', pointBorderWidth: 2, borderDash: [5,3] }
               ]
           },
           options: { responsive: true, maintainAspectRatio: true, plugins: { legend: { display: false } },
               scales: { x: { grid: { display: false }, ticks: { font: { size: 12 }, color: '#A8A8A8' } }, y: { beginAtZero: true, grid: { color: '#F0F2F5' }, ticks: { font: { size: 12 }, color: '#A8A8A8' } } } }
       });
       const legend = document.querySelector('.chart-legend');
       if (legend) legend.innerHTML = `<div class="legend-item"><span class="legend-dot" style="background:#0F62FE"></span>Scheduled</div><div class="legend-item"><span class="legend-dot" style="background:#198038"></span>Completed</div>`;
   }
   
   function initPipelineChart() {
       const el = document.getElementById('statusChart') || document.getElementById('pipelineChart');
       if (!el) return;
       const data = [180,120,42,38], labels = ['Applied','Shortlisted','Selected','Rejected'], colors = ['#0F62FE','#A56800','#198038','#A2191F'];
       new Chart(el, { type: 'doughnut', data: { labels, datasets: [{ data, backgroundColor: colors, borderWidth: 2, borderColor: '#fff' }] },
           options: { responsive: true, maintainAspectRatio: true, cutout: '68%', plugins: { legend: { display: false } } } });
       const legend = document.getElementById('pipelineLegend');
       if (legend) legend.innerHTML = labels.map((l,i) => `<div class="legend-item"><span class="legend-dot" style="background:${colors[i]}"></span>${l} (${data[i]})</div>`).join('');
   }
   
   let analyticsInited = false;
   function initAnalyticsCharts() {
       if (analyticsInited) return;
       analyticsInited = true;
       const monthly = document.getElementById('monthlyChart');
       if (monthly) new Chart(monthly, { type: 'bar',
           data: { labels: ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'],
               datasets: [{ label: 'Scheduled', data: [45,52,48,61,58,72,66,80,74,88,92,128], backgroundColor: '#0F62FE' },
                          { label: 'Completed',  data: [40,46,42,55,50,64,58,72,68,80,85,115], backgroundColor: '#198038' }] },
           options: { responsive: true, maintainAspectRatio: false,
               plugins: { legend: { position: 'bottom', labels: { font: { size: 12 }, boxWidth: 12 } } },
               scales: { x: { grid: { display: false }, ticks: { font: { size: 11 }, color: '#A8A8A8' } }, y: { beginAtZero: true, grid: { color: '#F0F2F5' }, ticks: { font: { size: 11 }, color: '#A8A8A8' } } } } });
       const dept = document.getElementById('deptChart');
       if (dept) new Chart(dept, { type: 'doughnut',
           data: { labels: ['Engineering','Product','Design','Data','Marketing'],
               datasets: [{ data: [42,18,14,16,10], backgroundColor: ['#0F62FE','#198038','#6929C4','#A56800','#005D5D'], borderWidth: 2, borderColor: '#fff' }] },
           options: { responsive: true, maintainAspectRatio: false, cutout: '60%', plugins: { legend: { position: 'bottom', labels: { font: { size: 12 }, boxWidth: 12 } } } } });
   }
   
   /* ── MINI CALENDAR ── */
   function buildMiniCalendar() {
       const cal = document.getElementById('miniCalendar');
       if (!cal) return;
       const today = new Date(), year = today.getFullYear(), month = today.getMonth();
       const days = ['Su','Mo','Tu','We','Th','Fr','Sa'];
       const dayHeaders = days.map(d => `<div class="cal-day-header">${d}</div>`).join('');
       const firstDay = new Date(year, month, 1).getDay();
       const daysInMonth = new Date(year, month+1, 0).getDate();
       const eventDays = [8,12,14,17,22];
       let cells = '';
       for (let i = 0; i < firstDay; i++) cells += `<div class="cal-day empty"></div>`;
       for (let d = 1; d <= daysInMonth; d++) {
           const isToday = d === today.getDate();
           const hasEvent = eventDays.includes(d);
           const cls = ['cal-day', isToday ? 'today' : '', hasEvent && !isToday ? 'has-event' : ''].filter(Boolean).join(' ');
           cells += `<div class="${cls}" onclick="selectCalDay(${d})">${d}</div>`;
       }
       cal.innerHTML = dayHeaders + cells;
   }
   function selectCalDay(day) {
       document.querySelectorAll('.cal-day').forEach(el => {
           if (+el.textContent === day && !el.classList.contains('today')) {
               el.style.background = '#EDF5FF'; el.style.color = '#0F62FE'; el.style.fontWeight = '600';
           }
       });
   }
   
   /* ── AI SCHEDULER ── */
   function runAISchedule() {
       const placeholder = document.getElementById('aiPlaceholder');
       const results     = document.getElementById('aiResults');
       if (placeholder && results) { placeholder.style.display = 'none'; results.classList.remove('hidden'); }
   }
   
   /* ── SETTINGS TABS ── */
   function initSettingsTabs() {
       document.querySelectorAll('.settings-menu-item').forEach(item => {
           item.addEventListener('click', () => {
               const tab = item.dataset.tab;
               document.querySelectorAll('.settings-menu-item').forEach(i => i.classList.remove('active'));
               item.classList.add('active');
               document.querySelectorAll('.settings-tab').forEach(t => t.classList.remove('active'));
               const panel = document.getElementById('settings-' + tab);
               if (panel) panel.classList.add('active');
           });
       });
   }
   
   /* ── UTILITY ── */
   function escHtml(str) {
       if (!str) return '';
       return String(str).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
   }
   
   console.log('%cInterviewPro loaded ✅', 'color:#0F62FE;font-weight:700;font-size:14px;');
   
   /* ── Analytics Data Loading ── */
   async function loadAnalyticsData() {
       try {
           const [ivs, conflicts] = await Promise.all([
               apiGet('/Interviews').catch(() => []),
               apiGet('/Conflicts').catch(() => [])
           ]);
           const total = ivs ? ivs.length : 0;
           const completed = ivs ? ivs.filter(i => i.status === 1).length : 0;
           const rate = total > 0 ? Math.round((completed / total) * 100) : 0;
           const avg = total > 0 ? (total / 30).toFixed(1) : '0';
           const conflictCount = conflicts ? conflicts.length : 0;
   
           const anTotal = document.getElementById('anTotal');
           const anRate  = document.getElementById('anRate');
           const anAvg   = document.getElementById('anAvg');
           const anConf  = document.getElementById('anConflicts');
           if (anTotal) anTotal.textContent = total;
           if (anRate)  anRate.textContent  = rate + '%';
           if (anAvg)   anAvg.textContent   = avg;
           if (anConf)  anConf.textContent  = conflictCount;
       } catch (e) {}
   }
   
   /* ── Save profile settings ── */
   function saveProfileSettings() {
       const raw = localStorage.getItem('user');
       if (raw) {
           try {
               const u = JSON.parse(raw);
               const nm = document.getElementById('settingsName');
               if (nm && nm.value.trim()) {
                   u.name = nm.value.trim();
                   localStorage.setItem('user', JSON.stringify(u));
                   loadUserInfo();
                   showToast('Profile updated successfully!', 'success');
               }
           } catch(e) {}
       }
   }