document.addEventListener('DOMContentLoaded', function() {
    // Check if user is already logged in
    const token = localStorage.getItem('token');
    if (token) {
        console.log("User is already logged in");
    }

    const loginForm = document.getElementById('loginForm');
    const errorMessage = document.getElementById('errorMessage');

    loginForm.addEventListener('submit', async function(e) {
        e.preventDefault();
        
        // Clear previous error messages
        errorMessage.textContent = '';
        
        // Get form values
        const username = document.getElementById('username').value;
        const password = document.getElementById('password').value;
        
        try {
            const response = await fetch('/Auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    username,
                    password
                })
            });
            
            const data = await response.json();
            
            if (response.ok) {
                // Save token and user info
                localStorage.setItem('token', data.token);
                localStorage.setItem('user', JSON.stringify(data.user));
                
                // Redirect based on user role
                if (data.user && data.user.role === "Admin") {
                    window.location.href = 'admin/dashboard.html'; // Redirect to admin dashboard
                } else if (data.user && data.user.role === "Staff") {
                    window.location.href = 'staff.html'; // Redirect to staff page in Staff folder
                } else {
                    window.location.href = 'home.html'; // Redirect to home page for non-admins
                }
            } else {
                errorMessage.textContent = data.message || 'Invalid username or password';
            }
        } catch (error) {
            console.error('Login error:', error);
            errorMessage.textContent = 'An error occurred. Please try again.';
        }
    });
});