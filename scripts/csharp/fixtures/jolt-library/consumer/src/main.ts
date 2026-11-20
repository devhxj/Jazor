import { createApp, nextTick } from 'vue';
import App from './App.vue';

function element(selector: string) {
    const result = document.querySelector<HTMLElement>(selector);
    if (!result) throw new Error(`Missing element ${selector}`);
    return result;
}

function assert(condition: boolean, message: string) {
    if (!condition) throw new Error(message);
}

async function verify() {
    const mount = createApp(App);
    mount.mount('#app');
    await nextTick();
    for (const [surface, initial, factor, caption] of [
        ['sfc', 2, 3, 'Add from SFC'],
        ['tsx', 5, 4, 'Increment']
    ] as const) {
        const button = element(`#${surface} .meter-button`);
        const reading = element(`#${surface} .meter-reading`);
        const parentValue = element(`#${surface} .parent-value`);
        assert(button.textContent?.trim() === caption, `${surface} optional prop`);
        assert(reading.textContent === String(initial), `${surface} initial slot value`);
        assert(getComputedStyle(button).color === 'rgb(21, 40, 180)', `${surface} scoped style`);
        assert(getComputedStyle(reading).fontWeight === '700', `${surface} slotted style`);
        button.click();
        await nextTick();
        assert(parentValue.textContent === String(initial + 1), `${surface} model update`);
        assert(reading.textContent === String(initial + 1), `${surface} updated slot value`);
        element(`#${surface} .multiply`).click();
        await nextTick();
        assert(element(`#${surface} .product`).textContent === String((initial + 1) * factor), `${surface} exposed method`);
    }
    const outside = element('#outside');
    assert(getComputedStyle(outside).color !== 'rgb(21, 40, 180)', 'Scoped style leaked');
    assert(getComputedStyle(outside).borderTopWidth === '7px', 'Missing global style');
    const stock = [...element('#stock').querySelectorAll('td')].map(cell => cell.textContent);
    assert(stock.join('|') === 'PAPER|3|6|stock', 'Generic slot did not render the typed item and nested record');
}

verify().then(() => element('#result').textContent = 'Passed',
    error => element('#result').textContent = String(error));
