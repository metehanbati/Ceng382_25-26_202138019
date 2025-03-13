// Bu bölüm yapay zeka kullanılarak yapılmıştır.

const classForm = document.getElementById('class-form');
const classTableBody = document.querySelector('#class-table tbody');

classForm.addEventListener('submit', function(event) {
    event.preventDefault();

    const className = document.getElementById('class-name').value;
    const numPeople = document.getElementById('num-people').value;
    const description = document.getElementById('description').value;

    addClassToTable(className, numPeople, description);
    classForm.reset();
});

function addClassToTable(className, numPeople, description) {
    const newRow = document.createElement('tr');
    newRow.innerHTML = `
        <td>${className}</td>
        <td>${numPeople}</td>
        <td>${description}</td>
    `;
    classTableBody.appendChild(newRow);
}

// Event Examples
classTableBody.addEventListener('click', function(event) {
    if (event.target.tagName === 'TD') {
        const row = event.target.parentNode;
        console.log('Row Clicked:', row);
        row.classList.toggle('highlighted');
    }
});

classTableBody.addEventListener('mouseover', function(event) {
    if (event.target.tagName === 'TD') {
        const row = event.target.parentNode;
        row.style.backgroundColor = '#ffffcc';
    }
});

classTableBody.addEventListener('mouseout', function(event) {
    if (event.target.tagName === 'TD') {
        const row = event.target.parentNode;
        row.style.backgroundColor = '';
    }
});

const inputs = document.querySelectorAll('input, textarea');
inputs.forEach(input => {
    input.addEventListener('focus', function() {
        this.style.borderColor = 'blue';
    });

    input.addEventListener('blur', function() {
        this.style.borderColor = '#ccc';
    });
});