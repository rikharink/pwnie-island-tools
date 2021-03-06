// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include "stdafx.h"
#include <windows.h>
#include <iostream>
#include <iomanip>
#include <stdio.h>
#include <stdlib.h>

// Detours imports
#include "detours.h"
#pragma comment(lib, "detours.lib")

// DX11 imports
#include <d3d11.h>
#include <D3Dcompiler.h>
#pragma comment(lib, "D3dcompiler.lib")
#pragma comment(lib, "d3d11.lib")
#pragma comment(lib, "winmm.lib")

//ImGUI imports
#include "imgui.h"
#include "imgui_impl_win32.h"
#include "imgui_impl_dx11.h"
#include "dllmain.h"
#include <vector>
#include <string>

#include "geom.h"
#include "mem.h"

// D3X HOOK DEFINITIONS
typedef HRESULT(__stdcall* IDXGISwapChainPresent)(IDXGISwapChain* pSwapChain, UINT SyncInterval, UINT Flags);
IDXGISwapChainPresent fnIDXGISwapChainPresent;

// Definition of WndProc Hook. Its here to avoid dragging dependencies on <windows.h> types.
extern LRESULT ImGui_ImplWin32_WndProcHandler(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam);
BOOL g_bInitialised = false;
bool g_ShowTrainer = false;
// Main D3D11 Objects
ID3D11DeviceContext* pContext = NULL;
ID3D11Device* pDevice = NULL;
ID3D11RenderTargetView* mainRenderTargetView;
static IDXGISwapChain* pSwapChain = NULL;
static WNDPROC OriginalWndProcHandler = nullptr;
HWND window = nullptr;


// TRAINER DEFINITIONS
uintptr_t moduleBase;
uintptr_t dllBase;

vec3* position = new vec3{ 0.0, 0.0, 0.0 };

vec3* cameraPosition = new vec3{ 0.0, 0.0, 0.0 };
vec4* cameraOrientation = new vec4{ 0.0, 0.0, 0.0, 0.0 };
vec3 cameraRotation;

typedef void(__fastcall* Chat)(void* pThis, void*, char* msg);
Chat fnChat;

typedef bool(__fastcall* FastTravelAvailable)(void* pThis, void*, void* player);
FastTravelAvailable fnFastTravelAvailable;

// HACK TOGGLES
bool g_flyHackEnabled = false;
bool g_unlockAllFastTravel = false;
float speed = 1.0;

LRESULT CALLBACK hWndProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	POINT mPos;
	GetCursorPos(&mPos);
	ScreenToClient(window, &mPos);
	ImGui::GetIO().MousePos.x = mPos.x;
	ImGui::GetIO().MousePos.y = mPos.y;

	if (uMsg == WM_KEYUP)
	{
		if (wParam == VK_DELETE)
		{
			g_ShowTrainer = !g_ShowTrainer;
		}
	}

	if (g_ShowTrainer)
	{
		ImGui::GetIO().MouseDrawCursor = true;
		ImGui_ImplWin32_WndProcHandler(hWnd, uMsg, wParam, lParam);
		return true;
	}
	else
	{
		ImGui::GetIO().MouseDrawCursor = false;
	}
	return CallWindowProc(OriginalWndProcHandler, hWnd, uMsg, wParam, lParam);
}

HRESULT GetDeviceAndCtxFromSwapchain(IDXGISwapChain* pSwapChain, ID3D11Device** ppDevice, ID3D11DeviceContext** ppContext)
{
	HRESULT ret = pSwapChain->GetDevice(__uuidof(ID3D11Device), (PVOID*)ppDevice);

	if (SUCCEEDED(ret))
		(*ppDevice)->GetImmediateContext(ppContext);

	return ret;
}

void ShowTrainer(bool* p_open)
{
	if (ImGuiCond_FirstUseEver) {
		position = (vec3*)mem::FindAddress(moduleBase + 0x018FCD60, { 0x20, 0x244, 0x20C, 0xC, 0xD4, 0x90 });
		cameraPosition = (vec3*)mem::FindAddress(moduleBase + 0x01684648, { 0x0, 0xAC, 0x88, 0x2E8, 0x4A4, 0xD4, 0x90 });
		cameraOrientation = (vec4*)mem::FindAddress(moduleBase + 0x01684648, { 0x0, 0xAC, 0x88, 0x2E8, 0x4A4, 0xD4, 0x80 });
	}

	// We specify a default position/size in case there's no data in the .ini file. Typically this isn't required! We only do it to make the Demo applications a little more welcoming.
	ImGui::SetNextWindowPos(ImVec2(650, 20), ImGuiCond_FirstUseEver);
	ImGui::SetNextWindowSize(ImVec2(550, 680), ImGuiCond_FirstUseEver);

	// Main body of the Demo window starts here.
	if (!ImGui::Begin("PwnieTrainer", p_open))
	{
		// Early out if the window is collapsed, as an optimization.
		ImGui::End();
		return;
	}

	ImGui::InputFloat3("Position", (float*)position);
	ImGui::InputFloat3("Camera Position", (float*)cameraPosition);
	ImGui::InputFloat4("Camera Orientation", (float*)cameraOrientation);
	ImGui::Checkbox("Noclip", &g_flyHackEnabled);
	ImGui::InputFloat("Noclip speed", &speed);
	ImGui::Checkbox("Unlock all FastTravel", &g_unlockAllFastTravel);


	ImGui::PushItemWidth(ImGui::GetFontSize() * -12);           // Use fixed width for labels (by passing a negative value), the rest goes to widgets. We choose a width proportional to our font size.
	ImGui::Spacing();
	ImGui::End();
}

HRESULT __stdcall Present(IDXGISwapChain* pChain, UINT SyncInterval, UINT Flags)
{
	if (!g_bInitialised) {
		std::cout << "[+] Present Hook called for the first time" << std::endl;
		if (FAILED(GetDeviceAndCtxFromSwapchain(pChain, &pDevice, &pContext)))
		{
			std::cout << "[+] Failed to get device and context from swapchain" << std::endl;
			return fnIDXGISwapChainPresent(pChain, SyncInterval, Flags);
		}
		pSwapChain = pChain;
		DXGI_SWAP_CHAIN_DESC sd;
		pChain->GetDesc(&sd);
		ImGui::CreateContext();
		ImGuiIO& io = ImGui::GetIO(); (void)io;
		io.ConfigFlags |= ImGuiConfigFlags_NavEnableKeyboard;
		window = sd.OutputWindow;

		//Set OriginalWndProcHandler to the Address of the Original WndProc function
		OriginalWndProcHandler = (WNDPROC)SetWindowLongPtr(window, GWLP_WNDPROC, (LONG_PTR)hWndProc);

		ImGui_ImplWin32_Init(window);
		ImGui_ImplDX11_Init(pDevice, pContext);
		ImGui::GetIO().ImeWindowHandle = window;

		ID3D11Texture2D* pBackBuffer;

		pChain->GetBuffer(0, __uuidof(ID3D11Texture2D), (LPVOID*)&pBackBuffer);
		pDevice->CreateRenderTargetView(pBackBuffer, NULL, &mainRenderTargetView);
		pBackBuffer->Release();

		g_bInitialised = true;
	}
	ImGui_ImplWin32_NewFrame();
	ImGui_ImplDX11_NewFrame();

	ImGui::NewFrame();
	//Trainer is displayed when g_ShowTrainer is TRUE
	if (g_ShowTrainer)
	{
		bool bShow = true;
		ShowTrainer(&bShow);
	}

	ImGui::EndFrame();
	ImGui::Render();
	pContext->OMSetRenderTargets(1, &mainRenderTargetView, NULL);
	ImGui_ImplDX11_RenderDrawData(ImGui::GetDrawData());

	return fnIDXGISwapChainPresent(pChain, SyncInterval, Flags);
}

void detourDirectX()
{
	std::cout << "[+] Calling DirectX Detour" << std::endl;
	DetourTransactionBegin();
	DetourUpdateThread(GetCurrentThread());
	DetourAttach(&(LPVOID&)fnIDXGISwapChainPresent, (PBYTE)Present);
	DetourTransactionCommit();
}

template <class T>
void SAFE_RELEASE(T* t)
{
	if (t)
	{
		t->Release();
	}
}

bool GetD3D11SwapchainDeviceContext(void** pSwapchainTable, size_t Size_Swapchain, void** pDeviceTable, size_t Size_Device, void** pContextTable, size_t Size_Context)
{
	WNDCLASSEX wc{ 0 };
	wc.cbSize = sizeof(wc);
	wc.lpfnWndProc = DefWindowProc;
	wc.lpszClassName = TEXT("dummy class");

	if (!RegisterClassEx(&wc))
	{
		return false;
	}

	HWND hWnd = CreateWindow(wc.lpszClassName, TEXT(""), WS_DISABLED, 0, 0, 0, 0, NULL, NULL, NULL, nullptr);

	DXGI_SWAP_CHAIN_DESC swapChainDesc{ 0 };
	swapChainDesc.BufferCount = 1;
	swapChainDesc.BufferDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
	swapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
	swapChainDesc.OutputWindow = hWnd;
	swapChainDesc.SampleDesc.Count = 1;
	swapChainDesc.BufferDesc.ScanlineOrdering = DXGI_MODE_SCANLINE_ORDER_UNSPECIFIED;
	swapChainDesc.BufferDesc.Scaling = DXGI_MODE_SCALING_UNSPECIFIED;
	swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_DISCARD;
	swapChainDesc.Windowed = TRUE;

	D3D_FEATURE_LEVEL featureLevel[] =
	{
		D3D_FEATURE_LEVEL_9_1,
		D3D_FEATURE_LEVEL_9_2,
		D3D_FEATURE_LEVEL_9_3,
		D3D_FEATURE_LEVEL_10_0,
		D3D_FEATURE_LEVEL_10_1,
		D3D_FEATURE_LEVEL_11_0,
		D3D_FEATURE_LEVEL_11_1,
		D3D_FEATURE_LEVEL_12_0,
		D3D_FEATURE_LEVEL_12_1
	};

	IDXGISwapChain* pDummySwapChain = nullptr;
	ID3D11Device* pDummyDevice = nullptr;
	ID3D11DeviceContext* pDummyContext = nullptr;

	if (FAILED(D3D11CreateDeviceAndSwapChain(nullptr, D3D_DRIVER_TYPE_HARDWARE, NULL, NULL, featureLevel, 1, D3D11_SDK_VERSION, &swapChainDesc, &pDummySwapChain, &pDummyDevice, nullptr, &pDummyContext)))
	{
		DestroyWindow(swapChainDesc.OutputWindow);
		UnregisterClass(wc.lpszClassName, GetModuleHandle(nullptr));

		return false;
	}

	if (pSwapchainTable && pDummySwapChain)
	{
		memcpy(pSwapchainTable, *reinterpret_cast<void***>(pDummySwapChain), Size_Swapchain);
	}

	if (pDeviceTable && pDummyDevice)
	{
		memcpy(pDeviceTable, *reinterpret_cast<void***>(pDummyDevice), Size_Device);
	}

	if (pContextTable && pDummyContext)
	{
		memcpy(pContextTable, *reinterpret_cast<void***>(pDummyContext), Size_Context);
	}

	SAFE_RELEASE(pDummySwapChain);
	SAFE_RELEASE(pDummyDevice);
	SAFE_RELEASE(pDummyContext);

	DestroyWindow(swapChainDesc.OutputWindow);
	UnregisterClass(wc.lpszClassName, GetModuleHandle(nullptr));

	return true;
}

void retrieveValues(DWORD_PTR ptrIDXGISwapChainPresent)
{
	DWORD_PTR hDxgi = (DWORD_PTR)GetModuleHandle(L"dxgi.dll");
	fnIDXGISwapChainPresent = (IDXGISwapChainPresent)ptrIDXGISwapChainPresent;
	std::cout << "[+] Present Addr: " << std::hex << fnIDXGISwapChainPresent << std::endl;
}

void printValues()
{
	std::cout << "[+] ID3D11DeviceContext Addr: " << std::hex << pContext << std::endl;
	std::cout << "[+] ID3D11Device Addr: " << std::hex << pDevice << std::endl;
	std::cout << "[+] ID3D11RenderTargetView Addr: " << std::hex << mainRenderTargetView << std::endl;
	std::cout << "[+] IDXGISwapChain Addr: " << std::hex << pSwapChain << std::endl;
}

FILE* out = new FILE();
FILE* err = new FILE();
FILE* in = new FILE();
void SetupConsole()
{
	AllocConsole();                             // Open a new console window
	SetConsoleTitle(L"[+] PwnAdventure 3 Trainer");
	freopen_s(&out, "CONOUT$", "w", stdout);      // Redirect stdout to CONOUT$
	freopen_s(&err, "CONOUT$", "w", stderr);      // Redirect stderr to CONOUT$
	freopen_s(&in, "CONIN$", "r", stdin);         // Redirect stdin to CONIN$
	std::cout << ("[+] Running under PwnAdventure3!") << std::endl;   // Console works!
}

void destroyConsole()
{
	fclose(out);
	fclose(err);
	fclose(in);
	FreeConsole();
}

std::vector<std::string> split(const std::string& str, const std::string& delim)
{
	std::vector<std::string> tokens;
	size_t prev = 0, pos = 0;
	do
	{
		pos = str.find(delim, prev);
		if (pos == std::string::npos) pos = str.length();
		std::string token = str.substr(prev, pos - prev);
		if (!token.empty()) tokens.push_back(token);
		prev = pos + delim.length();
	} while (pos < str.length() && prev < str.length());
	return tokens;
}

void __fastcall SendChat(void* pThis, void* d, char* msg)
{
	if (strncmp(msg, "#", 1) == 0)
	{
		std::string chatMessage = msg;
		chatMessage.erase(0, 1);
		int commandArgsSplit = chatMessage.find(" ");
		std::string command = chatMessage.substr(0, commandArgsSplit);
		std::string args = chatMessage.substr(commandArgsSplit, chatMessage.length());
		std::cout << "CMD: " << command << " ARG: " << args << std::endl;
		if (command == "tp")
		{
			std::vector<std::string> coords = split(args, ";");
			if (coords.size() >= 3) {
				position->x = std::stof(coords[0]);
				position->y = std::stof(coords[1]);
				position->z = std::stof(coords[2]);
			}
		}
		return;
	}
	return fnChat(pThis, d, msg);
}

bool __fastcall FastTravelAvailableDetour(void* pThis, void* d, void* player) {
	if (g_unlockAllFastTravel) {
		return true;
	}
	return fnFastTravelAvailable(pThis, d, player);
}

void enableFastTravelDetour()
{
	std::cout << "\t[-] Enabling FastTravel Detour" << std::endl;
	DetourTransactionBegin();
	DetourUpdateThread(GetCurrentThread());
	DetourAttach(&(LPVOID&)fnFastTravelAvailable, (PBYTE)FastTravelAvailableDetour);
	DetourTransactionCommit();
}

void enableChatDetour()
{
	std::cout << "\t[-] Enabling Chat Detour" << std::endl;
	DetourTransactionBegin();
	DetourUpdateThread(GetCurrentThread());
	DetourAttach(&(LPVOID&)fnChat, (PBYTE)SendChat);
	DetourTransactionCommit();
}

void enableTrainerDetours()
{
	std::cout << "[+] Enabling Trainer Detours:" << std::endl;
	fnChat = (Chat)(dllBase + 0x551A0);
	enableChatDetour();

	fnFastTravelAvailable = (FastTravelAvailable)(dllBase + 0x11e90);
	enableFastTravelDetour();
}

void removeDetours()
{
	DetourTransactionBegin();
	DetourUpdateThread(GetCurrentThread());
	DetourDetach(&(LPVOID&)fnIDXGISwapChainPresent, (PBYTE)Present);
	DetourDetach(&(LPVOID&)fnChat, (PBYTE)SendChat);
	DetourDetach(&(LPVOID&)fnFastTravelAvailable, (PBYTE)FastTravelAvailableDetour);
	DetourTransactionCommit();
}

DWORD WINAPI TrainerThread(HMODULE hModule)
{
	void* SwapChain[18];
	void* Device[40];
	void* Context[108];

	moduleBase = (uintptr_t)GetModuleHandle(L"PwnAdventure3-Win32-Shipping.exe");
	dllBase = (uintptr_t)GetModuleHandle(L"GameLogic.dll");

	if (GetD3D11SwapchainDeviceContext(SwapChain, sizeof(SwapChain), Device, sizeof(Device), Context, sizeof(Context)))
	{
		SetupConsole();
		retrieveValues((DWORD_PTR)SwapChain[8]);
		detourDirectX();
		enableTrainerDetours();
	}
	else
	{
		std::cout << "[+] Couldn't dump IDXGISwapChainPresent address" << std::endl;
		destroyConsole();
	}

	char* nopCamera = _strdup("\x90\x90\x90\x90\x90\x90\x90");
	Hack cameraHack = Hack::Hack(moduleBase + 0x8DB2D8, nopCamera);

	const int VK_W = 87;
	const int VK_S = 83;
	const int VK_A = 65;
	const int VK_D = 68;

	int c = 0;
	vec3 dir = vec3{ 0,0,0 };
	vec3 e = vec3{ 0, 0, 0 };

	while (true)
	{
		if (GetAsyncKeyState(VK_END) & 1)
		{
			break;
		}

		if (g_flyHackEnabled)
		{
			if (!cameraHack.bStatus) {
				cameraHack.Enable();
			}

			dir.x = 1 - 2 * (cameraOrientation->y * cameraOrientation->y + cameraOrientation->z * cameraOrientation->z);
			dir.y = 2 * (cameraOrientation->x * cameraOrientation->y + cameraOrientation->w * cameraOrientation->z);
			dir.z = 2 * (cameraOrientation->x * cameraOrientation->z - cameraOrientation->w * cameraOrientation->y);

			float mul = (0.8 / sqrt(dir.x * dir.x + dir.y * dir.y)) * 0.05;

			e.x = dir.x * speed * mul;
			e.y = dir.y * speed * mul;
			e.z = dir.z * speed * mul;
			

			if (GetAsyncKeyState(VK_RBUTTON)) {
				cameraPosition->x += e.x;
				cameraPosition->y += e.y;
				cameraPosition->z += e.z;
			}

			if (GetAsyncKeyState(VK_W))
			{
				cameraPosition->x += e.x;
				cameraPosition->y += e.y;
			}
			if (GetAsyncKeyState(VK_S)) {
				cameraPosition->x -= e.x;
				cameraPosition->y -= e.y;
			}
			if (GetAsyncKeyState(VK_A)) {
				cameraPosition->x += e.y;
				cameraPosition->y += -e.x;
			}
			if (GetAsyncKeyState(VK_D)) {
				cameraPosition->x += -e.y;
				cameraPosition->y += e.x;
			}
			if (GetAsyncKeyState(VK_SPACE)) {
				cameraPosition->z += speed * mul;
			}
			if (GetAsyncKeyState(VK_SHIFT)) {
				cameraPosition->z -= speed * mul;
			}

			position->x = cameraPosition->x;
			position->y = cameraPosition->y;
			position->z = cameraPosition->z - 64.0024f;
		}
		else
		{
			if (cameraHack.bStatus) {
				cameraHack.Disable();
			}
		}

		Sleep(5);
	}

	removeDetours();
	Sleep(100);
	destroyConsole();
	FreeLibraryAndExitThread(hModule, 0);
	return 0;
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved) {
	switch (ul_reason_for_call) {
	case DLL_PROCESS_ATTACH:
	{
		HANDLE hThread = CreateThread(nullptr, 0,
			(LPTHREAD_START_ROUTINE)TrainerThread, hModule, 0, 0);
		if (hThread != nullptr)
			CloseHandle(hThread);
		break;
	}
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}

	return TRUE;
}

