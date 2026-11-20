import { defineComponent, ref } from 'vue';
import Meter from '@jazor/library-meter-fixture/Meter';

export default defineComponent({
    setup() {
        const value = ref(5);
        const meter = ref<InstanceType<typeof Meter> | null>(null);
        const product = ref(0);
        return () => <div id="tsx">
            <Meter ref={meter} value={value.value} onUpdate:value={amount => value.value = amount}
                v-slots={{
                    reading: ({ amount }) => <output class="meter-reading">{amount.toFixed(0)}</output>
                }} />
            <output class="parent-value">{value.value}</output>
            <button class="multiply" type="button" onClick={() => product.value = meter.value?.Multiply(4) ?? 0}>Multiply</button>
            <output class="product">{product.value}</output>
        </div>;
    }
});
