import pymem
import pymem.process

try:
    pm = pymem.Pymem("Megabonk.exe")
    print("Oyuna başarıyla bağlanıldı.")
except Exception as e:
    print(f"Hata: Oyun bulunamadı veya yönetici olarak çalıştırılmadı. {e}")
    exit()

module = pymem.process.module_from_name(pm.process_handle, "UnityPlayer.dll").lpBaseOfDll

def get_pointer_address(base, offsets):
    addr = pm.read_longlong(base)
    for offset in offsets[:-1]:
        addr = pm.read_longlong(addr + offset)
    return addr + offsets[-1]

base_static = module + 0x01C6A7C8
my_offsets = [0x4C8, 0x10, 0x0, 0x10, 0x18, 0x0, 0x13C]

try:
    # Dinamik adresi bul
    dynamic_address = get_pointer_address(base_static, my_offsets)
    print(f"Hesaplanan Dinamik Adres: {hex(dynamic_address)}")
    try:
        pm.write_int(dynamic_address,0)
        print("God mode açıldı.")
        while True:
            pm.write_int(dynamic_address,0)
    except Exception as e:
        print(f"God mode açılırken bir sorun oluştu: {e}")

except Exception as e:
    print(f"Adres hesaplanırken hata oluştu (Oyun kapalı veya harita yüklenmemiş olabilir): {e}")