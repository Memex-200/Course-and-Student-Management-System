import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { vi } from 'vitest';
import StudentAccount from './StudentAccount';
import { AuthProvider } from '../../contexts/AuthContext';
import { authAPI } from '../../lib/api';

// Mock the API calls
vi.mock('../../lib/api', () => ({
  authAPI: {
    updateUser: vi.fn()
  }
}));

// Mock the toast notifications
vi.mock('react-hot-toast', () => ({
  success: vi.fn(),
  error: vi.fn()
}));

describe('StudentAccount Component', () => {
  const mockUser = {
    id: 1,
    username: 'testuser',
    fullName: 'Test User',
    email: 'test@example.com',
    phone: '0123456789',
    userRole: 'Student',
    branchId: 1,
    branch: 'فرع أسيوط'
  };

  const mockSetUser = vi.fn();

  beforeEach(() => {
    // Reset all mocks before each test
    vi.clearAllMocks();
    
    // Setup localStorage mock
    const localStorageMock = {
      getItem: vi.fn(),
      setItem: vi.fn(),
      clear: vi.fn()
    };
    global.localStorage = localStorageMock;
  });

  test('renders user information correctly', () => {
    render(
      <AuthProvider value={{ user: mockUser, setUser: mockSetUser }}>
        <StudentAccount />
      </AuthProvider>
    );

    expect(screen.getByText(mockUser.fullName)).toBeInTheDocument();
    expect(screen.getByDisplayValue(mockUser.email)).toBeInTheDocument();
    expect(screen.getByDisplayValue(mockUser.phone)).toBeInTheDocument();
  });

  test('enables form fields when edit button is clicked', () => {
    render(
      <AuthProvider value={{ user: mockUser, setUser: mockSetUser }}>
        <StudentAccount />
      </AuthProvider>
    );

    const editButton = screen.getByText('تعديل');
    fireEvent.click(editButton);

    const fullNameInput = screen.getByDisplayValue(mockUser.fullName);
    const emailInput = screen.getByDisplayValue(mockUser.email);
    const phoneInput = screen.getByDisplayValue(mockUser.phone);

    expect(fullNameInput).not.toBeDisabled();
    expect(emailInput).not.toBeDisabled();
    expect(phoneInput).not.toBeDisabled();
  });

  test('updates user data successfully', async () => {
    const updatedUser = {
      ...mockUser,
      fullName: 'Updated Name',
      email: 'updated@example.com',
      phone: '9876543210'
    };

    // Mock the API response
    (authAPI.updateUser as any).mockResolvedValueOnce({
      data: {
        token: 'new-token',
        user: updatedUser
      }
    });

    render(
      <AuthProvider value={{ user: mockUser, setUser: mockSetUser }}>
        <StudentAccount />
      </AuthProvider>
    );

    // Click edit button
    const editButton = screen.getByText('تعديل');
    fireEvent.click(editButton);

    // Update form fields
    const fullNameInput = screen.getByDisplayValue(mockUser.fullName);
    const emailInput = screen.getByDisplayValue(mockUser.email);
    const phoneInput = screen.getByDisplayValue(mockUser.phone);

    fireEvent.change(fullNameInput, { target: { value: updatedUser.fullName } });
    fireEvent.change(emailInput, { target: { value: updatedUser.email } });
    fireEvent.change(phoneInput, { target: { value: updatedUser.phone } });

    // Click save button
    const saveButton = screen.getByText('حفظ');
    fireEvent.click(saveButton);

    // Verify API was called with correct data
    await waitFor(() => {
      expect(authAPI.updateUser).toHaveBeenCalledWith({
        fullName: updatedUser.fullName,
        email: updatedUser.email,
        phone: updatedUser.phone
      });
    });

    // Verify user context was updated
    await waitFor(() => {
      expect(mockSetUser).toHaveBeenCalledWith(updatedUser);
    });

    // Verify localStorage was updated
    await waitFor(() => {
      expect(localStorage.setItem).toHaveBeenCalledWith('token', 'new-token');
      expect(localStorage.setItem).toHaveBeenCalledWith('user', JSON.stringify(updatedUser));
    });
  });

  test('handles API errors correctly', async () => {
    // Mock API error
    (authAPI.updateUser as any).mockRejectedValueOnce({
      response: {
        data: {
          message: 'خطأ في تحديث البيانات'
        }
      }
    });

    render(
      <AuthProvider value={{ user: mockUser, setUser: mockSetUser }}>
        <StudentAccount />
      </AuthProvider>
    );

    // Click edit button
    const editButton = screen.getByText('تعديل');
    fireEvent.click(editButton);

    // Update form field
    const fullNameInput = screen.getByDisplayValue(mockUser.fullName);
    fireEvent.change(fullNameInput, { target: { value: 'New Name' } });

    // Click save button
    const saveButton = screen.getByText('حفظ');
    fireEvent.click(saveButton);

    // Verify error handling
    await waitFor(() => {
      expect(screen.getByText('خطأ في تحديث البيانات')).toBeInTheDocument();
    });

    // Verify user context was not updated
    expect(mockSetUser).not.toHaveBeenCalled();
  });
}); 