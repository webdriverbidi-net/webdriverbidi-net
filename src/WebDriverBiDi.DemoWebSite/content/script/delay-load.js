document.counter = 0;
window.addEventListener("load", (e) => {
  setTimeout(() => doLoadCycle(), 0);
});

function doLoadCycle() {
  if (document.counter < 3) {
    document.querySelector("div#contentMessage").innerText = document.querySelector("div#contentMessage").innerText + ".";
    document.counter++;
    setTimeout(() => {
      doLoadCycle();
    }, 1000);
  } else {
    const element = document.querySelector("div#contentMessage");
    element.innerText = "Loaded!";
    element.classList.add("page-loaded");
    if (document.onFinish) {
      document.onFinish();
    }
    document.customLatch = true;
  }
}
