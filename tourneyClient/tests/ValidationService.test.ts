
import { describe, test, expect } from 'vitest';
import { validateInput } from '../src/services/ValidationService';

describe('validationService', () =>
{

  test('detects HTML injection', () =>
  {
    const result = validateInput('<script>alert("xss")</script>');
    expect(result.isValid).toBe(false);

  });

  test('detects SQL injection', () =>
  {
    const result = validateInput("SELECT * FROM users WHERE '1'='1';");
    expect(result.isValid).toBe(false);
  });

  test('detects JavaScript injection', () =>
  {
    const result = validateInput('document.cookie = "steal";');
    expect(result.isValid).toBe(false);
  });

  test('detects open redirect', () =>
  {
    const result = validateInput('https://malicious-site.com');
    expect(result.isValid).toBe(false);

  });

  test('detects escape character misuse', () =>
  {
    const result = validateInput("Robert'); DROP TABLE Students;--");
    expect(result.isValid).toBe(false);

  });

  test('passes clean input', () =>
  {
    const result = validateInput('HelloWorld123');
    expect(result.isValid).toBe(true);

  });
});
