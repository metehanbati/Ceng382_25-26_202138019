document.getElementById('login-form').addEventListener('submit', function(event) {
    event.preventDefault();

    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;

    if (username === 'admin' && password === 'admin') {
        window.location.href = 'table.html'; // Giriş başarılıysa table.html'ye yönlendir
    } else {
        alert('Kullanıcı adı veya şifre hatalı!');
    }
});