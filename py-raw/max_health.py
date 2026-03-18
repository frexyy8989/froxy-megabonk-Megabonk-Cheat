import pymem
import pymem.process

try:
    pm = pymem.Pymem("Megabonk.exe")
    print("Oyuna başarıyla bağlanıldı.")
except Exception as e:
    print(f"Hata: Oyun bulunamadı veya yönetici olarak çalıştırılmadı. {e}")
    exit()

module = pymem.process.module_from_name(pm.process_handle, "GameAssembly.dll").lpBaseOfDll

def get_pointer_address(base, offsets):
    addr = pm.read_longlong(base)
    for offset in offsets[:-1]:
        addr = pm.read_longlong(addr + offset)
    return addr + offsets[-1]

base_static = module + 0x02F60EA8
my_offsets = [0x20, 0xB8, 0x30, 0x78, 0x48, 0x20, 0x14]

try:
    dynamic_address = get_pointer_address(base_static, my_offsets)
    print(f"Hesaplanan Dinamik Adres: {hex(dynamic_address)}")

    current_value = pm.read_int(dynamic_address)
    print(f"Şuanki maksimum can: {current_value}")
    i = input("Ayarlanacak maksimum can: ")
    pm.write_int(dynamic_address,int(i))
    print(f"Yeni maksimum can: {i}")

except Exception as e:
    print(f"Adres hesaplanırken hata oluştu (Oyun kapalı veya harita yüklenmemiş olabilir): {e}")