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

base_static = module + 0x02F6B020
my_offsets = [0x40, 0xB8, 0x0, 0x78, 0x48, 0x40, 0x10]

try:
    dynamic_address = get_pointer_address(base_static, my_offsets)
    print(f"Hesaplanan Dinamik Adres: {hex(dynamic_address)}")
    try:
        i = int(input("Kaç cana ayarlanacak: "))
        pm.write_int(dynamic_address,i)
    except Exception as e:
        print(f"Can değiştirirken bir hata oluştu: {e}")

except Exception as e:
    print(f"Adres hesaplanırken hata oluştu (Oyun kapalı veya harita yüklenmemiş olabilir): {e}")