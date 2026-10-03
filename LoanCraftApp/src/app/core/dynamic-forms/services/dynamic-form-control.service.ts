import { Injectable } from '@angular/core';
import { FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { DynamicControlBase } from '../models/dynamic-control-base';

@Injectable({ providedIn: 'root' })
export class DynamicFormControlService {
    toFormGroup(controls: DynamicControlBase<any>[]): FormGroup {
        const group: Record<string, FormControl> = {};

        controls.forEach((control) => {
            const validators: ValidatorFn[] = [];
            if (control.required) validators.push(Validators.required);
            if (control.controlType === 'number') {
                if (control.min !== undefined) validators.push(Validators.min(control.min));
                if (control.max !== undefined) validators.push(Validators.max(control.max));
            }

            group[control.key] = new FormControl(
                control.value !== undefined ? control.value : '',
                validators
            );
        });

        return new FormGroup(group);
    }
}
