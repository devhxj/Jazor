declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    cell: {
        type: import("vue").PropType<import("./type").TdCalendarProps["cell"]>;
    };
    cellAppend: {
        type: import("vue").PropType<import("./type").TdCalendarProps["cellAppend"]>;
    };
    controllerConfig: {
        type: import("vue").PropType<import("./type").TdCalendarProps["controllerConfig"]>;
        default: import("./type").TdCalendarProps["controllerConfig"];
    };
    fillWithZero: {
        type: BooleanConstructor;
        default: any;
    };
    firstDayOfWeek: {
        type: NumberConstructor;
        validator(val: import("./type").TdCalendarProps["firstDayOfWeek"]): boolean;
    };
    format: {
        type: StringConstructor;
        default: string;
    };
    head: {
        type: import("vue").PropType<import("./type").TdCalendarProps["head"]>;
    };
    isShowWeekendDefault: {
        type: BooleanConstructor;
        default: boolean;
    };
    mode: {
        type: import("vue").PropType<import("./type").TdCalendarProps["mode"]>;
        default: import("./type").TdCalendarProps["mode"];
        validator(val: import("./type").TdCalendarProps["mode"]): boolean;
    };
    month: {
        type: import("vue").PropType<import("./type").TdCalendarProps["month"]>;
    };
    multiple: BooleanConstructor;
    preventCellContextmenu: BooleanConstructor;
    range: {
        type: import("vue").PropType<import("./type").TdCalendarProps["range"]>;
    };
    theme: {
        type: import("vue").PropType<import("./type").TdCalendarProps["theme"]>;
        default: import("./type").TdCalendarProps["theme"];
        validator(val: import("./type").TdCalendarProps["theme"]): boolean;
    };
    value: {
        type: import("vue").PropType<import("./type").TdCalendarProps["value"]>;
    };
    week: {
        type: import("vue").PropType<import("./type").TdCalendarProps["week"]>;
    };
    year: {
        type: import("vue").PropType<import("./type").TdCalendarProps["year"]>;
    };
    onCellClick: import("vue").PropType<import("./type").TdCalendarProps["onCellClick"]>;
    onCellDoubleClick: import("vue").PropType<import("./type").TdCalendarProps["onCellDoubleClick"]>;
    onCellRightClick: import("vue").PropType<import("./type").TdCalendarProps["onCellRightClick"]>;
    onControllerChange: import("vue").PropType<import("./type").TdCalendarProps["onControllerChange"]>;
    onMonthChange: import("vue").PropType<import("./type").TdCalendarProps["onMonthChange"]>;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    cell: {
        type: import("vue").PropType<import("./type").TdCalendarProps["cell"]>;
    };
    cellAppend: {
        type: import("vue").PropType<import("./type").TdCalendarProps["cellAppend"]>;
    };
    controllerConfig: {
        type: import("vue").PropType<import("./type").TdCalendarProps["controllerConfig"]>;
        default: import("./type").TdCalendarProps["controllerConfig"];
    };
    fillWithZero: {
        type: BooleanConstructor;
        default: any;
    };
    firstDayOfWeek: {
        type: NumberConstructor;
        validator(val: import("./type").TdCalendarProps["firstDayOfWeek"]): boolean;
    };
    format: {
        type: StringConstructor;
        default: string;
    };
    head: {
        type: import("vue").PropType<import("./type").TdCalendarProps["head"]>;
    };
    isShowWeekendDefault: {
        type: BooleanConstructor;
        default: boolean;
    };
    mode: {
        type: import("vue").PropType<import("./type").TdCalendarProps["mode"]>;
        default: import("./type").TdCalendarProps["mode"];
        validator(val: import("./type").TdCalendarProps["mode"]): boolean;
    };
    month: {
        type: import("vue").PropType<import("./type").TdCalendarProps["month"]>;
    };
    multiple: BooleanConstructor;
    preventCellContextmenu: BooleanConstructor;
    range: {
        type: import("vue").PropType<import("./type").TdCalendarProps["range"]>;
    };
    theme: {
        type: import("vue").PropType<import("./type").TdCalendarProps["theme"]>;
        default: import("./type").TdCalendarProps["theme"];
        validator(val: import("./type").TdCalendarProps["theme"]): boolean;
    };
    value: {
        type: import("vue").PropType<import("./type").TdCalendarProps["value"]>;
    };
    week: {
        type: import("vue").PropType<import("./type").TdCalendarProps["week"]>;
    };
    year: {
        type: import("vue").PropType<import("./type").TdCalendarProps["year"]>;
    };
    onCellClick: import("vue").PropType<import("./type").TdCalendarProps["onCellClick"]>;
    onCellDoubleClick: import("vue").PropType<import("./type").TdCalendarProps["onCellDoubleClick"]>;
    onCellRightClick: import("vue").PropType<import("./type").TdCalendarProps["onCellRightClick"]>;
    onControllerChange: import("vue").PropType<import("./type").TdCalendarProps["onControllerChange"]>;
    onMonthChange: import("vue").PropType<import("./type").TdCalendarProps["onMonthChange"]>;
}>> & Readonly<{}>, {
    format: string;
    mode: "month" | "year";
    multiple: boolean;
    theme: "card" | "full";
    controllerConfig: boolean | import("./type").CalendarController;
    isShowWeekendDefault: boolean;
    fillWithZero: boolean;
    preventCellContextmenu: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
