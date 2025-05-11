// document.addEventListener('DOMContentLoaded', function() {
//     // Check if user is already logged in
//     const token = localStorage.getItem('token');
//     if (token) {
//         console.log("Registered successfully");
//     }

//     const registerForm = document.getElementById('registerForm');
//     const errorMessage = document.getElementById('errorMessage');

//     registerForm.addEventListener('submit', async function(e) {
//         e.preventDefault();
        
//         // Clear previous error messages
//         errorMessage.textContent = '';
        
//         // Get form values
//         const username = document.getElementById('username').value;
//         const email = document.getElementById('email').value;
//         const password = document.getElementById('password').value;
//         const confirmPassword = document.getElementById('confirmPassword').value;
        
//         // Validate passwords match
//         if (password !== confirmPassword) {
//             errorMessage.textContent = 'Passwords do not match';
//             return;
//         }
        
//         try {
//             const response = await fetch('/Auth/register', {
//                 method: 'POST',
//                 headers: {
//                     'Content-Type': 'application/json'
//                 },
//                 body: JSON.stringify({
//                     username,
//                     email,
//                     password,
//                     confirmPassword
//                 })
//             });
            
//             const data = await response.json();
            
//             if (response.ok) {
//                 // Redirect to login page with success message
//                 window.location.href = 'Login.html?registered=true';
//             } else {
//                 errorMessage.textContent = data.message || 'Registration failed';
//             }
//         } catch (error) {
//             console.error('Registration error:', error);
//             errorMessage.textContent = 'An error occurred. Please try again.';
//         }
//     });
// });

document.addEventListener('DOMContentLoaded', function() {
    // Check if user is already logged in
    const token = localStorage.getItem('token');
    if (token) {
        console.log("Registered successfully");
    }

    const registerForm = document.getElementById('registerForm');
    const errorMessage = document.getElementById('errorMessage');

    registerForm.addEventListener('submit', async function(e) {
        e.preventDefault();
        
        // Clear previous error messages
        errorMessage.textContent = '';
        
        // Get form values
        const username = document.getElementById('Username').value;
        const email = document.getElementById('Email').value;
        const password = document.getElementById('Password').value;
        const confirmPassword = document.getElementById('ConfirmPassword').value;
        
        // Validate passwords match
        if (password !== confirmPassword) {
            errorMessage.textContent = 'Passwords do not match';
            return;
        }
        
        try {
            // Add console log to see what's being sent
            const requestBody = {
                Username: username,
                Email: email,
                Password: password,
                ConfirmPassword: confirmPassword
            };
            console.log('Sending registration data:', requestBody);
            
            const response = await fetch('/Auth/register', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(requestBody)
            });
            
            // Log the response status
            console.log('Response status:', response.status);
            
            const data = await response.json();
            console.log('Response data:', data);
            
            if (response.ok) {
                // Redirect to login page with success message
                window.location.href = 'Login.html?registered=true';
            } else {
                // Handle specific error messages from the server
                if (data.errors) {
                    // If there are validation errors, display them
                    const errorMessages = [];
                    for (const key in data.errors) {
                        errorMessages.push(data.errors[key].join(', '));
                    }
                    errorMessage.textContent = errorMessages.join('. ');
                } else if (data.message) {
                    errorMessage.textContent = data.message;
                } else {
                    errorMessage.textContent = 'Registration failed. Please try again.';
                   alert('Registration failed. Please try again.');

                }
            }
        } catch (error) {
            console.error('Registration error:', error);
            errorMessage.textContent = 'An error occurred. Please try again.';
        }
    });
});