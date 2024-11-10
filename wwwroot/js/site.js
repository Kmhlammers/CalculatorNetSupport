function appendToDisplay(value) {
    let display = document.getElementById('display');
    let errorMessage = document.getElementById('error-message');

    if (errorMessage) {
        errorMessage.innerText = '';
    }
    display.value += value;
    display.scrollLeft = display.scrollWidth;
}

function clearDisplay() {
    document.getElementById('display').value = '';
    let errorMessage = document.getElementById('error-message');

    if (errorMessage) {
        errorMessage.innerText = '';
    }

}

function deleteLastCharacter() {
    let display = document.getElementById('display');
    let errorMessage = document.getElementById('error-message');

    if (errorMessage) {
        errorMessage.innerText = '';
    }

    display.value = display.value.slice(0, -1);
    
}

function scrollDisplayLeft() {
    let display = document.getElementById('display');
    display.scrollLeft -= 20; 
}

function scrollDisplayRight() {
    let display = document.getElementById('display');
    display.scrollLeft += 20; 
}

// Submit the form programmatically when enter is pressed
document.getElementById('calculator').addEventListener('keypress', function (event) {
    if (event.key === 'Enter') {
        event.preventDefault(); 
        document.getElementById("form").submit();
        
    }
});