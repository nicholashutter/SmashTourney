
import { describe, test, expect } from 'vitest';
import { validationService } from '../src/services/validationService';

describe('validationService', () => {
  const { validateInput } = validationService();

  test('detects HTML injection', () => {
    const result = validateInput('<script>alert("xss")</script>');
    expect(result.isValid).toBe(false);
    expect(result.issues).toContain('Potential HTML injection detected.');
  });

  test('detects SQL injection', () => {
    const result = validateInput("SELECT * FROM users WHERE '1'='1';");
    expect(result.isValid).toBe(false);
    expect(result.issues).toContain('Potential SQL injection detected.');
  });

  test('detects JavaScript injection', () => {
    const result = validateInput('document.cookie = "steal";');
    expect(result.isValid).toBe(false);
    expect(result.issues).toContain('Potential JavaScript injection detected.');
  });

  test('detects open redirect', () => {
    const result = validateInput('https://malicious-site.com');
    expect(result.isValid).toBe(false);
    expect(result.issues).toContain('Potential open redirect detected.');
  });

  test('detects escape character misuse', () => {
    const result = validateInput("Robert'); DROP TABLE Students;--");
    expect(result.isValid).toBe(false);
    expect(result.issues).toContain('Potential escape character misuse detected.');
  });

  test('passes clean input', () => {
    const result = validateInput('HelloWorld123');
    expect(result.isValid).toBe(true);
    expect(result.issues.length).toBe(0);
  });
});
