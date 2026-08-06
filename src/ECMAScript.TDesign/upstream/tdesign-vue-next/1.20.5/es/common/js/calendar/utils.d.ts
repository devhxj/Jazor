import dayjs from 'dayjs';
import type { CalendarValue } from './types';
export declare const createDefaultCurDate: () => dayjs.Dayjs;
export declare const handleRange: (range?: Array<CalendarValue>) => {
    from: CalendarValue;
    to: CalendarValue;
} | null;
