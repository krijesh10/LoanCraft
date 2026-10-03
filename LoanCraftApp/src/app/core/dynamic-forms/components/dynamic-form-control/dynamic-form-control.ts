import { Component, Input } from '@angular/core';
import { DynamicControlBase } from '../../models/dynamic-control-base';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dynamic-form-control',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './dynamic-form-control.html',
  styleUrl: './dynamic-form-control.css',
})
export class DynamicFormControl {
  @Input({ required: true }) control!: DynamicControlBase<any>;
  @Input({ required: true }) form!: FormGroup;

  get isValid(): boolean {
    const c = this.form.get(this.control.key);
    return c ? c.valid || c.untouched : true;
  }
  // Dynamic error message based on active validator errors
  get errorMessage(): string {
    const c = this.form.get(this.control.key);
    if (!c || !c.errors) return '';
    if (c.errors['required']) return `${this.control.label} is required`;
    if (c.errors['min']) return `Minimum value is ${c.errors['min'].min}`;
    if (c.errors['max']) return `Maximum value is ${c.errors['max'].max}`;
    return 'Invalid input';
  }
}
