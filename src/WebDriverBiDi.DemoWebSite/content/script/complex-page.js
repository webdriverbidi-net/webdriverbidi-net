// Toggle button functionality for kitten image
document.querySelector("#toggle-button").addEventListener("click", e => {
  e.stopPropagation();
  document.querySelector("img").classList.toggle("hide-pic");
});

// Modal overlay functionality
window.addEventListener("load", () => {
  const overlay = document.getElementById("modal-overlay");
  const countdownElement = document.getElementById("countdown-timer");
  
  // Show the overlay
  overlay.classList.add("visible");
  
  let countdown = 3;
  
  // Update countdown every second
  const countdownInterval = setInterval(() => {
    countdown--;
    if (countdownElement) {
      countdownElement.textContent = `${countdown} second${countdown === 1 ? "" : "s"}`;
    }
    
    if (countdown <= 0) {
      clearInterval(countdownInterval);
      // Remove the overlay by adding hidden class (removes pointer-events)
      overlay.classList.remove("visible");
      overlay.classList.add("hidden");
      
      // Enable the toggle button 1 second after the overlay disappears
      setTimeout(() => {
        const toggleButton = document.getElementById("toggle-button");
        if (toggleButton) {
          toggleButton.disabled = false;
        }
      }, 1000);
    }
  }, 1000);
});
