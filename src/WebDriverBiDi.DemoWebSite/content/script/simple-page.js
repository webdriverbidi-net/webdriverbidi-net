document.querySelector("#toggle-button").addEventListener("click", e => {
  e.stopPropagation();
  document.querySelector("img").classList.toggle("hide-pic");
});
