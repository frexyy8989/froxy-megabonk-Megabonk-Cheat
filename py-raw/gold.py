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

base_static = module + 0x02F77F00
my_offsets = [0x40, 0x80, 0x30, 0xB8, 0x68, 0x90, 0x68]

try:
    dynamic_address = get_pointer_address(base_static, my_offsets)
    print(f"Hesaplanan Dinamik Adres: {hex(dynamic_address)}")
    current_value = pm.read_float(dynamic_address)
    print(f"Mevcut gold miktarı: {current_value}")
    i = input("Eklenecek gold coin miktarı: ")
    pm.write_float(dynamic_address,int(i)+current_value)
    print(f"Eklenen coin miktarı: {i}")
except Exception as e:
    print(f"Adres hesaplanırken hata oluştu (Oyun kapalı veya harita yüklenmemiş olabilir): {e}")