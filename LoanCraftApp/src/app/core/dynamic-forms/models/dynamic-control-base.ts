export interface DynamicControlConfig<T> {
    value?: T;
    key?: string;
    label?: string;
    required?: boolean;
    order?: number;
    controlType?: 'textbox' | 'number' | 'dropdown' | 'switch' | 'textarea';
    type?: string;
    options?: { label: string; value: any }[];
    placeholder?: string;
    min?: number;
    max?: number;
}

export class DynamicControlBase<T = any> {
    value: T | undefined;
    key: string;
    label: string;
    required: boolean;
    order: number;
    controlType: 'textbox' | 'number' | 'dropdown' | 'switch' | 'textarea';
    type: string;
    options: { label: string; value: any }[];
    placeholder: string;
    min?: number;
    max?: number;

    constructor(options: DynamicControlConfig<T> = {}) {
        this.value = options.value;
        this.key = options.key || '';
        this.label = options.label || '';
        this.required = !!options.required;
        this.order = options.order === undefined ? 1 : options.order;
        this.controlType = options.controlType || 'textbox';
        this.type = options.type || '';
        this.options = options.options || [];
        this.placeholder = options.placeholder || '';
        this.min = options.min;
        this.max = options.max;
    }
}

// Concrete Control Classes
export class TextboxControl extends DynamicControlBase<string> {
    override controlType = 'textbox' as const;
}

export class NumberControl extends DynamicControlBase<number> {
    override controlType = 'number' as const;
}

export class DropdownControl extends DynamicControlBase<string> {
    override controlType = 'dropdown' as const;
}

export class SwitchControl extends DynamicControlBase<boolean> {
    override controlType = 'switch' as const;
}
