import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnChanges,
  SimpleChanges,
  inject,
  OnDestroy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { DynamicControlBase } from '../../models/dynamic-control-base';
import { DynamicFormControlService } from '../../services/dynamic-form-control.service';
import { DynamicFormControl } from '../dynamic-form-control/dynamic-form-control';

@Component({
  selector: 'app-dynamic-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DynamicFormControl],
  templateUrl: './dynamic-form.html',
  styleUrl: './dynamic-form.css',
})
export class DynamicForm implements OnChanges, OnDestroy {
  // Injects our factory service from Step 1.2
  private readonly formService = inject(DynamicFormControlService);

  // The array of control metadata schemas passed by the canvas
  @Input({ required: true }) controls: DynamicControlBase<any>[] = [];
  @Input() submitLabel = 'Save';
  @Input() showSubmitButton = false;

  // Emits real-time value changes back to the canvas as the user types
  @Output() formChange = new EventEmitter<{ values: any; valid: boolean }>();

  // Emits full form value when user clicks submit button
  @Output() formSubmit = new EventEmitter<any>();

  form: FormGroup = new FormGroup({});
  private valueChangesSub?: Subscription;

  // Lifecycle hook: triggers whenever the parent passes a NEW controls array
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['controls']) {
      this.rebuildForm();
    }
  }

  private rebuildForm(): void {
    // 1. Unsubscribe from any previous form listeners to prevent memory leaks
    this.valueChangesSub?.unsubscribe();

    // 2. Synthesize a brand new FormGroup from the metadata array
    this.form = this.formService.toFormGroup(this.controls);

    // 3. Listen to live user input with 150ms debounce
    this.valueChangesSub = this.form.valueChanges
      .pipe(debounceTime(150))
      .subscribe((values) => {
        this.formChange.emit({
          values,
          valid: this.form.valid,
        });
      });
  }

  onSubmit(): void {
    if (this.form.valid) {
      this.formSubmit.emit(this.form.value);
    } else {
      this.form.markAllAsTouched();
    }
  }

  ngOnDestroy(): void {
    this.valueChangesSub?.unsubscribe();
  }
}
