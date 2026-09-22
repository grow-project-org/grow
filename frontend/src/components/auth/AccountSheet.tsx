import { BottomSheet } from '../sheet/BottomSheet';
import { Button } from '../ui/Button';
import { useAuth } from '../../state/AuthContext';
import { useGarden } from '../../state/GardenContext';
import sheet from '../sheet/sheetForm.module.css';

interface AccountSheetProps {
  open: boolean;
  onClose: () => void;
}

export const AccountSheet = ({ open, onClose }: AccountSheetProps) => {
  const { username } = useAuth();
  const { plants, species, groups } = useGarden();

  return (
    <BottomSheet open={open} onClose={onClose}>
      <h2 className={sheet.title}>Konto</h2>
      <p className={sheet.desc}>
        Zalogowano jako <strong>{username}</strong>.
        <br />
        {plants.length} roślin · {species.length} gatunków · {groups.length} grup.
      </p>
      <div className={sheet.actions}>
        <Button variant="neutral" onClick={onClose} block>
          Zamknij
        </Button>
      </div>
    </BottomSheet>
  );
};
