import { renderHook, act } from '@testing-library/react-hooks';
import useFormValidation from './useFormValidation';

describe('useFormValidation Hook', () => {
  const initialValues = {
    name: '',
    email: '',
    age: 0
  };
  
  const validateFn = (values) => {
    const errors = {};
    
    if (!values.name) {
      errors.name = 'Name is required';
    }
    
    if (!values.email) {
      errors.email = 'Email is required';
    } else if (!/^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i.test(values.email)) {
      errors.email = 'Invalid email address';
    }
    
    if (values.age < 18) {
      errors.age = 'Must be at least 18';
    }
    
    return errors;
  };
  
  const onSubmit = jest.fn();
  
  beforeEach(() => {
    onSubmit.mockClear();
  });
  
  test('initializes with initial values', () => {
    const { result } = renderHook(() => useFormValidation(initialValues, validateFn, onSubmit));
    
    expect(result.current.values).toEqual(initialValues);
    expect(result.current.errors).toEqual({});
    expect(result.current.touched).toEqual({});
    expect(result.current.isSubmitting).toBe(false);
    expect(result.current.submitError).toBe(null);
    expect(result.current.submitSuccess).toBe(false);
  });
  
  test('handles input change', () => {
    const { result } = renderHook(() => useFormValidation(initialValues, validateFn, onSubmit));
    
    act(() => {
      result.current.handleChange({
        target: { name: 'name', value: 'John Doe' }
      });
    });
    
    expect(result.current.values.name).toBe('John Doe');
  });
  
  test('handles checkbox change', () => {
    const { result } = renderHook(() => useFormValidation(
      { ...initialValues, isActive: false },
      validateFn,
      onSubmit
    ));
    
    act(() => {
      result.current.handleChange({
        target: { name: 'isActive', type: 'checkbox', checked: true }
      });
    });
    
    expect(result.current.values.isActive).toBe(true);
  });
  
  test('handles custom value change', () => {
    const { result } = renderHook(() => useFormValidation(initialValues, validateFn, onSubmit));
    
    act(() => {
      result.current.handleCustomChange('age', 25);
    });
    
    expect(result.current.values.age).toBe(25);
  });
  
  test('validates on blur', () => {
    const { result } = renderHook(() => useFormValidation(initialValues, validateFn, onSubmit));
    
    // Set a value first
    act(() => {
      result.current.handleChange({
        target: { name: 'email', value: 'invalid-email' }
      });
    });
    
    // Trigger blur
    act(() => {
      result.current.handleBlur({
        target: { name: 'email' }
      });
    });
    
    expect(result.current.touched.email).toBe(true);
    expect(result.current.errors.email).toBe('Invalid email address');
  });
  
  test('clears errors when field is changed', () => {
    const { result } = renderHook(() => useFormValidation(initialValues, validateFn, onSubmit));
    
    // Set a value and trigger validation
    act(() => {
      result.current.handleChange({
        target: { name: 'email', value: 'invalid-email' }
      });
      result.current.handleBlur({
        target: { name: 'email' }
      });
    });
    
    expect(result.current.errors.email).toBe('Invalid email address');
    
    // Change the value again
    act(() => {
      result.current.handleChange({
        target: { name: 'email', value: 'valid@email.com' }
      });
    });
    
    expect(result.current.errors.email).toBeUndefined();
  });
  
  test('validates all fields on submit', async () => {
    const { result } = renderHook(() => useFormValidation(initialValues, validateFn, onSubmit));
    
    await act(async () => {
      await result.current.handleSubmit();
    });
    
    expect(result.current.errors.name).toBe('Name is required');
    expect(result.current.errors.email).toBe('Email is required');
    expect(result.current.errors.age).toBe('Must be at least 18');
    expect(Object.values(result.current.touched).every(t => t === true)).toBe(true);
    expect(onSubmit).not.toHaveBeenCalled();
  });
  
  test('submits when validation passes', async () => {
    const { result } = renderHook(() => useFormValidation(initialValues, validateFn, onSubmit));
    
    // Set valid values
    act(() => {
      result.current.setMultipleValues({
        name: 'John Doe',
        email: 'john@example.com',
        age: 25
      });
    });
    
    await act(async () => {
      await result.current.handleSubmit();
    });
    
    expect(result.current.errors).toEqual({});
    expect(onSubmit).toHaveBeenCalledWith({
      name: 'John Doe',
      email: 'john@example.com',
      age: 25
    });
    expect(result.current.isSubmitting).toBe(false);
    expect(result.current.submitSuccess).toBe(true);
  });
  
  test('handles submit error', async () => {
    const errorMessage = 'Submission failed';
    const onSubmitWithError = jest.fn().mockRejectedValue(new Error(errorMessage));
    
    const { result } = renderHook(() => useFormValidation(
      { name: 'John', email: 'john@example.com', age: 25 },
      validateFn,
      onSubmitWithError
    ));
    
    await act(async () => {
      await result.current.handleSubmit();
    });
    
    expect(onSubmitWithError).toHaveBeenCalled();
    expect(result.current.submitError).toBe(errorMessage);
    expect(result.current.submitSuccess).toBe(false);
    expect(result.current.isSubmitting).toBe(false);
  });
  
  test('resets the form', () => {
    const { result } = renderHook(() => useFormValidation(initialValues, validateFn, onSubmit));
    
    // Change values and trigger errors
    act(() => {
      result.current.handleChange({
        target: { name: 'name', value: 'John' }
      });
      result.current.handleBlur({
        target: { name: 'email' }
      });
      result.current.setSubmitError('Some error');
      result.current.setSubmitSuccess(true);
    });
    
    // Reset the form
    act(() => {
      result.current.resetForm();
    });
    
    expect(result.current.values).toEqual(initialValues);
    expect(result.current.errors).toEqual({});
    expect(result.current.touched).toEqual({});
    expect(result.current.submitError).toBe(null);
    expect(result.current.submitSuccess).toBe(false);
  });
  
  test('sets field value', () => {
    const { result } = renderHook(() => useFormValidation(initialValues, validateFn, onSubmit));
    
    act(() => {
      result.current.setFieldValue('name', 'John Doe');
    });
    
    expect(result.current.values.name).toBe('John Doe');
  });
  
  test('sets multiple values at once', () => {
    const { result } = renderHook(() => useFormValidation(initialValues, validateFn, onSubmit));
    
    act(() => {
      result.current.setMultipleValues({
        name: 'John Doe',
        email: 'john@example.com'
      });
    });
    
    expect(result.current.values.name).toBe('John Doe');
    expect(result.current.values.email).toBe('john@example.com');
    expect(result.current.values.age).toBe(0); // Unchanged
  });
});
