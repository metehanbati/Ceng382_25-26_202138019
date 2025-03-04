const loginAttempts = [];
let formsHidden = false;

document.getElementById('login-form').addEventListener('submit', function(event) {
    event.preventDefault();

    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;

    loginAttempts.push({ username, password });
    console.log(loginAttempts);

    if (username === 'Ash' && password === 'Pikachu') {
        alert('Giriş başarılı!');
    } else {
        alert('Kullanıcı adı veya şifre hatalı!');
    }
});

function updateClock() {
    const now = new Date();
    const timeString = now.toLocaleTimeString();
    document.getElementById('live-clock').textContent = timeString;
}

updateClock();
setInterval(updateClock, 1000);

document.addEventListener('keydown', function(event) {
    if (event.key === 'h' || event.key === 'H') {
        formsHidden = !formsHidden;
        const forms = document.querySelectorAll('form');
        forms.forEach(form => {
            form.style.display = formsHidden ? 'none' : 'block';
        });
    }
});