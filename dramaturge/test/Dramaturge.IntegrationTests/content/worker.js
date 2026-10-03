fetch('data.txt').then((r) => r.text()).then((t) => postMessage(t), () => postMessage('failed'));
