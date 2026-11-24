export function getErrorMessage(err: unknown, fallback: string): string {
  const error = err as { error?: unknown; message?: unknown };
  const errorBody = error.error as { message?: unknown; errors?: unknown } | undefined;

  if (typeof errorBody?.message === 'string' && errorBody.message.trim()) {
    return errorBody.message;
  }

  const validationMessage = getValidationErrorMessage(errorBody?.errors);
  if (validationMessage) return validationMessage;

  if (typeof error.error === 'string' && error.error.trim()) return error.error;
  if (typeof error.message === 'string' && error.message.trim()) return error.message;
  return fallback;
}

function getValidationErrorMessage(errors: unknown): string {
  if (!errors || typeof errors !== 'object') return '';

  const first = Object.values(errors).flat().find(Boolean);
  if (typeof first === 'string') return first;
  if (typeof first === 'number' || typeof first === 'boolean') return first.toString();
  return '';
}
