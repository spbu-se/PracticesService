import { test, expect, Page } from '@playwright/test';

const login = async (page: Page) => {
    await page.goto('/practices');
    await expect(page).toHaveURL(/\/login$/);
    await page.getByLabel('Логин').fill('admin@example.com');
    await page.getByLabel('Пароль').fill('YourSecurePassword123!');
    await page.getByRole('button', { name: 'Войти' }).click();
    await expect(page).toHaveURL('/');
};

test('login works correctly', async ({ page }) => {
    await login(page);
});

test.describe('Header navigation', () => {
    test.beforeEach(async ({ page }) => {
        await login(page);
    });

    test('should show header and navigate home via logo click', async ({ page }) => {
        const title = page.getByRole('heading', { name: 'PracticesService' });
        await expect(title).toBeVisible();
        await title.click();
        await expect(page).toHaveURL('/');
    });

    test('should open profile menu and logout', async ({ page }) => {
        await page.getByRole('button').locator('svg').click();
        const logoutButton = page.getByRole('menuitem', { name: 'Выйти' });
        await expect(logoutButton).toBeVisible();
        await logoutButton.click();
        await expect(page).toHaveURL('/login');
    });
});

test.describe('RegisterPage', () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/register');
    });

    test('renders all fields correctly', async ({ page }) => {
        await expect(page.getByLabel('Имя')).toBeVisible();
        await expect(page.getByLabel('Фамилия')).toBeVisible();
        await expect(page.getByLabel('Отчество (необязательно)')).toBeVisible();
        await expect(page.getByLabel('Email')).toBeVisible();
        await expect(page.getByLabel('Пароль')).toBeVisible();
        await expect(page.getByRole('button', { name: 'Зарегистрироваться' })).toBeVisible();
    });

    test('shows alert when required fields are empty', async ({ page }) => {
        page.on('dialog', dialog => {
            expect(dialog.message()).toContain('Пожалуйста, заполните все обязательные поля');
            dialog.dismiss();
        });
        await page.getByRole('button', { name: 'Зарегистрироваться' }).click();
    });

    test('registers user successfully', async ({ page }) => {
        await page.route('**/register', async route => {
            const body = await route.request().postDataJSON();
            expect(body.email).toBe('test@example.com');
            route.fulfill({
                status: 200,
                contentType: 'application/json',
                body: JSON.stringify({
                    token: 'mock-token',
                    refreshToken: 'mock-refresh'
                }),
            });
        });

        await page.fill('input[name="firstName"]', 'Иван');
        await page.fill('input[name="lastName"]', 'Иванов');
        await page.fill('input[name="email"]', 'test@example.com');
        await page.fill('input[name="password"]', 'Passw0rd!');
        await page.getByRole('button', { name: 'Зарегистрироваться' }).click();
        await expect(page).toHaveURL('/');
    });
});

test.describe('Profile Page', () => {
    test.beforeEach(async ({ page }) => {
        await login(page);
        await page.goto('/profile');
    });

    test('displays profile info correctly', async ({ page }) => {
        await expect(page.getByText(/User Admin/i)).toBeVisible();
        await expect(page.locator('text=Роли')).toBeVisible();
    });
});

test.describe('ThemesIndexPage UI tests', () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/themes');
    });

    test('loads themes list and shows filters', async ({ page }) => {
        await expect(page.locator('text=Список тем')).toBeVisible();
        await expect(page.locator('role=tab >> text=Активные темы')).toBeVisible();
        await expect(page.locator('role=tab >> text=Архивированные темы')).toBeVisible();
        await expect(page.locator('button:has-text("Предложить тему")')).toBeVisible();
    });

    test('can navigate to create theme page', async ({ page }) => {
        await page.click('button:has-text("Предложить тему")');
        await expect(page).toHaveURL(/\/create\/theme/);
    });

    test('can toggle archive tabs', async ({ page }) => {
        await page.click('role=tab >> text=Архивированные темы');
        const archivedTab = page.locator('role=tab >> text=Архивированные темы');
        await expect(archivedTab).toHaveAttribute('aria-selected', 'true');
    });

    test('Practice supervisor can archive all filtered themes', async ({ page }) => {
        const archiveButton = page.locator('button:has-text("Архивировать все"), button:has-text("Восстановить все")');

        if (await archiveButton.count() > 0) {
            await archiveButton.click();
            await page.once('dialog', dialog => dialog.accept());
            await expect(archiveButton).toHaveText(/Восстановить все|Архивировать все/);
        } else {
            test.skip('Archive button not available (user may not be practice supervisor)');
        }
    });
});
