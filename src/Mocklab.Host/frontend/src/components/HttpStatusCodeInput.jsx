import { useEffect, useMemo, useState } from 'react';
import { AutoComplete } from 'primereact/autocomplete';
import { httpStatusCodeOptions, parseHttpStatusCode } from '../utils/httpStatusCodes';

const formatStatusCodeValue = (value) => {
  if (value === null || value === undefined || value === '') {
    return '';
  }

  const matchedOption = httpStatusCodeOptions.find((option) => option.value === value);
  return matchedOption ? matchedOption.label : String(value);
};

export function HttpStatusCodeInput({
  value,
  onChange,
  placeholder = 'Select or enter status code',
  className,
  style,
  inputId
}) {
  const [inputValue, setInputValue] = useState(formatStatusCodeValue(value));
  const [suggestions, setSuggestions] = useState(httpStatusCodeOptions.map((option) => option.label));

  useEffect(() => {
    setInputValue(formatStatusCodeValue(value));
  }, [value]);

  const allSuggestions = useMemo(
    () => httpStatusCodeOptions.map((option) => option.label),
    []
  );

  const commitValue = (rawValue) => {
    const nextValue = parseHttpStatusCode(rawValue, value ?? 200);
    onChange(nextValue);
    setInputValue(formatStatusCodeValue(nextValue));
  };

  const handleComplete = (event) => {
    const query = (event.query || '').trim().toLowerCase();
    if (!query) {
      setSuggestions(allSuggestions);
      return;
    }

    setSuggestions(
      allSuggestions.filter((option) => option.toLowerCase().includes(query))
    );
  };

  return (
    <AutoComplete
      inputId={inputId}
      value={inputValue}
      suggestions={suggestions}
      completeMethod={handleComplete}
      dropdown
      dropdownMode="blank"
      onChange={(e) => setInputValue(e.value)}
      onSelect={(e) => commitValue(e.value)}
      onBlur={() => commitValue(inputValue)}
      onKeyDown={(e) => {
        if (e.key === 'Enter') {
          commitValue(inputValue);
        }
      }}
      placeholder={placeholder}
      className={className}
      style={style}
    />
  );
}
