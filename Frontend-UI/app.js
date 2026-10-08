

// API Configuration
const API_BASE_URL = 'http://localhost:5000/api';
let authToken = localStorage.getItem('authToken');

document.addEventListener('DOMContentLoaded', function() {
    initializeApp();
});

function initializeApp() {
    setupNavigation();
    setupMenuToggle();
    initializeCharts();
    checkAuthentication();
}

function setupNavigation() {
    const navItems = document.querySelectorAll('.nav-item');
    
    navItems.forEach(item => {
        item.addEventListener('click', function(e) {
            e.preventDefault();
            
            // Remove active class from all items
            navItems.forEach(nav => nav.classList.remove('active'));
            
            // Add active class to clicked item
            this.classList.add('active');
            
            // Get page name
            const pageName = this.getAttribute('data-page');
            
            // Switch page
            switchPage(pageName);
        });
    });
}

function switchPage(pageName) {
    // Hide all pages
    const pages = document.querySelectorAll('.page-content');
    pages.forEach(page => page.classList.remove('active'));
    
    // Show selected page
    const selectedPage = document.getElementById(pageName);
    if (selectedPage) {
        selectedPage.classList.add('active');
    } else {
        // Load page dynamically
        loadPageContent(pageName);
    }
}

// Mobile Menu Toggle
function setupMenuToggle() {
    const menuToggle = document.querySelector('.menu-toggle');
    const sidebar = document.querySelector('.sidebar');
    
    if (menuToggle) {
        menuToggle.addEventListener('click', function() {
            sidebar.classList.toggle('active');
        });
    }
}

// ============================================
// CHARTS INITIALIZATION
// ============================================
function initializeCharts() {
    // Interview Trends Chart
    const ctxInterview = document.getElementById('interviewChart');
    if (ctxInterview) {
        new Chart(ctxInterview, {
            type: 'line',
            data: {
                labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
                datasets: [{
                    label: 'Scheduled',
                    data: [12, 19, 15, 25, 22, 18, 24],
                    borderColor: '#667eea',
                    backgroundColor: 'rgba(102, 126, 234, 0.1)',
                    tension: 0.4,
                    fill: true
                }, {
                    label: 'Completed',
                    data: [10, 15, 13, 20, 18, 15, 20],
                    borderColor: '#10b981',
                    backgroundColor: 'rgba(16, 185, 129, 0.1)',
                    tension: 0.4,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: {
                    legend: {
                        position: 'bottom'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    }

    // Candidate Status Chart
    const ctxStatus = document.getElementById('statusChart');
    if (ctxStatus) {
        new Chart(ctxStatus, {
            type: 'doughnut',
            data: {
                labels: ['Applied', 'Shortlisted', 'Rejected'],
                datasets: [{
                    data: [180, 120, 42],
                    backgroundColor: [
                        '#667eea',
                        '#10b981',
                        '#ef4444'
                    ],
                    borderWidth: 0
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: {
                    legend: {
                        position: 'bottom'
                    }
                }
            }
        });
    }
}

function loadPageContent(pageName) {
    const container = document.getElementById('pageContainer');
    let content = '';

    switch(pageName) {
        case 'interviews':
            content = getInterviewsPage();
            break;
        case 'candidates':
            content = getCandidatesPage();
            break;
        case 'interviewers':
            content = getInterviewersPage();
            break;
        case 'availability':
            content = getAvailabilityPage();
            break;
        case 'conflicts':
            content = getConflictsPage();
            break;
        case 'analytics':
            content = getAnalyticsPage();
            break;
        case 'reports':
            content = getReportsPage();
            break;
        case 'settings':
            content = getSettingsPage();
            break;
        default:
            content = '<div class="page-content active"><h1>Page not found</h1></div>';
    }

    // Append new page if it doesn't exist
    if (!document.getElementById(pageName)) {
        container.insertAdjacentHTML('beforeend', content);
    }

    // Show the page
    switchPage(pageName);
}

function getInterviewsPage() {
    return `
        <div class="page-content" id="interviews">
            <div class="page-header">
                <div>
                    <h1>Interview Management</h1>
                    <p>Schedule, manage, and track all interviews</p>
                </div>
                <button class="btn-primary" onclick="showScheduleModal()">
                    <i class="fas fa-plus"></i> Schedule New Interview
                </button>
            </div>

            <div class="filters-bar">
                <input type="date" class="filter-input">
                <select class="filter-select">
                    <option>All Status</option>
                    <option>Scheduled</option>
                    <option>Completed</option>
                    <option>Cancelled</option>
                </select>
                <select class="filter-select">
                    <option>All Interviewers</option>
                </select>
                <button class="btn-primary">Apply Filters</button>
            </div>

            <div class="recent-activity">
                <div class="table-responsive">
                    <table class="data-table">
                        <thead>
                            <tr>
                                <th>ID</th>
                                <th>Candidate</th>
                                <th>Interviewer</th>
                                <th>Position</th>
                                <th>Date & Time</th>
                                <th>Duration</th>
                                <th>Status</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody id="interviewsTableBody">
                            <tr>
                                <td>#001</td>
                                <td>
                                    <div class="user-cell">
                                        <img src="https://ui-avatars.com/api/?name=John+Doe" alt="">
                                        <span>John Doe</span>
                                    </div>
                                </td>
                                <td>Sarah Johnson</td>
                                <td>Senior Developer</td>
                                <td>Feb 18, 2025 - 10:00 AM</td>
                                <td>60 mins</td>
                                <td><span class="badge-success">Scheduled</span></td>
                                <td>
                                    <button class="btn-icon" onclick="viewInterview(1)"><i class="fas fa-eye"></i></button>
                                    <button class="btn-icon" onclick="editInterview(1)"><i class="fas fa-edit"></i></button>
                                    <button class="btn-icon" onclick="cancelInterview(1)"><i class="fas fa-times"></i></button>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    `;
}

function getCandidatesPage() {
    return `
        <div class="page-content" id="candidates">
            <div class="page-header">
                <div>
                    <h1>Candidate Management</h1>
                    <p>Manage all candidate profiles and applications</p>
                </div>
                <button class="btn-primary" onclick="addCandidate()">
                    <i class="fas fa-user-plus"></i> Add Candidate
                </button>
            </div>

            <div class="stats-grid">
                <div class="stat-card blue">
                    <div class="stat-icon"><i class="fas fa-user-check"></i></div>
                    <div class="stat-info">
                        <h3>180</h3>
                        <p>Applied</p>
                    </div>
                </div>
                <div class="stat-card green">
                    <div class="stat-icon"><i class="fas fa-user-graduate"></i></div>
                    <div class="stat-info">
                        <h3>120</h3>
                        <p>Shortlisted</p>
                    </div>
                </div>
                <div class="stat-card red">
                    <div class="stat-icon"><i class="fas fa-user-times"></i></div>
                    <div class="stat-info">
                        <h3>42</h3>
                        <p>Rejected</p>
                    </div>
                </div>
            </div>

            <div class="recent-activity">
                <div class="table-responsive">
                    <table class="data-table">
                        <thead>
                            <tr>
                                <th>Candidate</th>
                                <th>Email</th>
                                <th>Phone</th>
                                <th>Position Applied</th>
                                <th>Applied Date</th>
                                <th>Status</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            <!-- Candidate data will be loaded here -->
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    `;
}

function getInterviewersPage() {
    return `
        <div class="page-content" id="interviewers">
            <div class="page-header">
                <div>
                    <h1>Interviewer Management</h1>
                    <p>Manage interviewer profiles and availability</p>
                </div>
                <button class="btn-primary">
                    <i class="fas fa-user-plus"></i> Add Interviewer
                </button>
            </div>

            <div class="recent-activity">
                <div class="table-responsive">
                    <table class="data-table">
                        <thead>
                            <tr>
                                <th>Interviewer</th>
                                <th>Department</th>
                                <th>Total Interviews</th>
                                <th>Available Slots</th>
                                <th>Rating</th>
                                <th>Status</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            <!-- Interviewer data will be loaded here -->
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    `;
}

function getAvailabilityPage() {
    return `
        <div class="page-content" id="availability">
            <div class="page-header">
                <div>
                    <h1>Availability Management</h1>
                    <p>Manage interviewer time slots and schedules</p>
                </div>
                <button class="btn-primary">
                    <i class="fas fa-clock"></i> Add Availability
                </button>
            </div>

            <div class="calendar-container">
                <!-- Calendar will be integrated here -->
                <p>Calendar view coming soon...</p>
            </div>
        </div>
    `;
}

function getConflictsPage() {
    return `
        <div class="page-content" id="conflicts">
            <div class="page-header">
                <h1>Conflict Resolution</h1>
                <p>AI-powered conflict detection and resolution</p>
            </div>

            <div class="alert-box warning">
                <i class="fas fa-exclamation-triangle"></i>
                <div>
                    <h4>8 Conflicts Detected</h4>
                    <p>Review and resolve scheduling conflicts</p>
                </div>
            </div>

            <!-- Conflicts list will be shown here -->
        </div>
    `;
}

function getAnalyticsPage() {
    return `
        <div class="page-content" id="analytics">
            <div class="page-header">
                <h1>Analytics & Insights</h1>
                <p>Detailed analytics and performance metrics</p>
            </div>
            <!-- Analytics charts and metrics -->
        </div>
    `;
}

function getReportsPage() {
    return `
        <div class="page-content" id="reports">
            <div class="page-header">
                <h1>Reports</h1>
                <p>Generate and download comprehensive reports</p>
            </div>
            <!-- Report generation interface -->
        </div>
    `;
}

function getSettingsPage() {
    return `
        <div class="page-content" id="settings">
            <div class="page-header">
                <h1>Settings</h1>
                <p>Configure system preferences and notifications</p>
            </div>
            <!-- Settings forms -->
        </div>
    `;
}

function checkAuthentication() {
    if (!authToken) {
        // Redirect to login if not authenticated
        // window.location.href = 'login.html';
    }
}

async function fetchInterviews() {
    // API call to get interviews
}

async function scheduleInterview(data) {
    // API call to schedule interview
}

async function cancelInterview(id) {
    if (confirm('Are you sure you want to cancel this interview?')) {
        // API call to cancel
        console.log('Cancelling interview:', id);
    }
}

// Placeholder functions
function showScheduleModal() {
    alert('Schedule interview modal would open here');
}

function viewInterview(id) {
    console.log('View interview:', id);
}

function editInterview(id) {
    console.log('Edit interview:', id);
}

function addCandidate() {
    alert('Add candidate form would open here');
}

console.log('Interview Scheduling System Loaded Successfully!');